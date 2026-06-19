const { contextBridge, ipcRenderer } = require('electron');
const { pathToFileURL } = require('url');

contextBridge.exposeInMainWorld('hotelVideoWall', {
  listVideos: async () => {
    const videos = await ipcRenderer.invoke('videos:list');
    return videos.map((videoPath) => pathToFileURL(videoPath).href);
  },
  getConfig: () => ipcRenderer.invoke('config:get')
});
