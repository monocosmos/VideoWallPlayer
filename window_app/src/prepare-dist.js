const fs = require('fs');
const path = require('path');

const distVideosDirectory = path.join(__dirname, '..', 'dist', 'videos');

fs.mkdirSync(distVideosDirectory, { recursive: true });
fs.writeFileSync(path.join(distVideosDirectory, '.gitkeep'), '');

console.log(`Video klasoru hazirlandi: ${distVideosDirectory}`);
