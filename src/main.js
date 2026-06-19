const { app, BrowserWindow, Menu, ipcMain, powerSaveBlocker } = require('electron');
const path = require('path');
const fs = require('fs/promises');

const SUPPORTED_EXTENSIONS = new Set(['.mp4', '.webm', '.mov', '.m4v', '.ogv', '.ogg']);
const isKiosk = process.argv.includes('--kiosk') || process.env.HOTEL_VIDEO_WALL_KIOSK === '1';
const isMuted = process.env.HOTEL_VIDEO_WALL_MUTED === '1';

let mainWindow;
let powerSaveBlockerId;

app.commandLine.appendSwitch('autoplay-policy', 'no-user-gesture-required');

function getVideoDirectory() {
  if (process.env.HOTEL_VIDEO_WALL_DIR) {
    return process.env.HOTEL_VIDEO_WALL_DIR;
  }

  if (app.isPackaged) {
    return path.join(path.dirname(app.getPath('exe')), 'videos');
  }

  return path.join(app.getAppPath(), 'videos');
}

async function listVideos() {
  const videoDirectory = getVideoDirectory();

  try {
    const entries = await fs.readdir(videoDirectory, { withFileTypes: true });

    return entries
      .filter((entry) => entry.isFile())
      .map((entry) => entry.name)
      .filter((name) => SUPPORTED_EXTENSIONS.has(path.extname(name).toLowerCase()))
      .sort((a, b) => a.localeCompare(b, undefined, { numeric: true, sensitivity: 'base' }))
      .map((name) => path.join(videoDirectory, name));
  } catch (error) {
    if (error.code === 'ENOENT') {
      await fs.mkdir(videoDirectory, { recursive: true });
      return [];
    }

    throw error;
  }
}

function createWindow() {
  Menu.setApplicationMenu(null);

  mainWindow = new BrowserWindow({
    fullscreen: true,
    kiosk: isKiosk,
    frame: false,
    autoHideMenuBar: true,
    backgroundColor: '#000000',
    show: false,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
    mainWindow.focus();
  });

  mainWindow.loadFile(path.join(__dirname, 'renderer', 'index.html'));
}

app.whenReady().then(() => {
  ipcMain.handle('videos:list', listVideos);
  ipcMain.handle('config:get', () => ({ muted: isMuted }));
  powerSaveBlockerId = powerSaveBlocker.start('prevent-display-sleep');
  createWindow();
});

app.on('window-all-closed', () => {
  if (powerSaveBlockerId) {
    powerSaveBlocker.stop(powerSaveBlockerId);
  }

  app.quit();
});
