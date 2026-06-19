const player = document.getElementById('player');

let playlist = [];
let currentIndex = 0;
let retryTimer;

function playCurrentVideo() {
  clearTimeout(retryTimer);

  if (playlist.length === 0) {
    player.removeAttribute('src');
    player.load();
    retryTimer = setTimeout(loadPlaylist, 5000);
    return;
  }

  player.src = playlist[currentIndex];
  player.load();

  const playPromise = player.play();
  if (playPromise) {
    playPromise.catch(() => {
      retryTimer = setTimeout(playCurrentVideo, 1000);
    });
  }
}

function playNextVideo() {
  if (playlist.length === 0) {
    loadPlaylist();
    return;
  }

  currentIndex = (currentIndex + 1) % playlist.length;
  playCurrentVideo();
}

async function loadPlaylist() {
  try {
    playlist = await window.hotelVideoWall.listVideos();
    currentIndex = currentIndex % Math.max(playlist.length, 1);
    playCurrentVideo();
  } catch {
    retryTimer = setTimeout(loadPlaylist, 5000);
  }
}

player.controls = false;
player.addEventListener('ended', playNextVideo);
player.addEventListener('error', playNextVideo);

window.hotelVideoWall.getConfig().then((config) => {
  player.muted = config.muted;
  loadPlaylist();
});
