const fs = require('fs');
const path = require('path');

const supportedExtensions = new Set(['.mp4', '.webm', '.mov', '.m4v', '.ogv', '.ogg']);
const videoDirectory = process.env.HOTEL_VIDEO_WALL_DIR || path.join(__dirname, '..', 'videos');

if (!fs.existsSync(videoDirectory)) {
  console.log(`Video klasoru yok: ${videoDirectory}`);
  process.exit(0);
}

const videos = fs
  .readdirSync(videoDirectory, { withFileTypes: true })
  .filter((entry) => entry.isFile())
  .map((entry) => entry.name)
  .filter((name) => supportedExtensions.has(path.extname(name).toLowerCase()))
  .sort((a, b) => a.localeCompare(b, undefined, { numeric: true, sensitivity: 'base' }));

if (videos.length === 0) {
  console.log(`Video bulunamadi: ${videoDirectory}`);
  process.exit(0);
}

console.log(`${videos.length} video bulundu:`);
for (const video of videos) {
  console.log(`- ${video}`);
}
