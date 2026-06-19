package com.monocosmos.videowallplayer;

import android.app.Activity;
import android.content.ClipData;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.net.Uri;
import android.os.Bundle;
import android.view.Gravity;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowInsets;
import android.view.WindowInsetsController;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.NumberPicker;
import android.widget.ScrollView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONException;
import org.videolan.libvlc.LibVLC;
import org.videolan.libvlc.Media;
import org.videolan.libvlc.MediaPlayer;
import org.videolan.libvlc.util.VLCVideoLayout;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Random;
import java.util.Set;

public final class MainActivity extends Activity {
    private static final int RequestVideos = 1001;
    private static final String PrefsName = "videowallplayer.android";
    private static final String KeyPlaylist = "playlist";
    private static final String KeyRepeat = "repeat";
    private static final String KeyShuffle = "shuffle";
    private static final String KeyMuted = "muted";
    private static final String KeyHardware = "hardware";
    private static final String KeyCache = "cache";

    private static final int RepeatAll = 0;
    private static final int RepeatOne = 1;
    private static final int RepeatNone = 2;

    private final ArrayList<String> playlist = new ArrayList<>();
    private final ArrayList<Integer> playOrder = new ArrayList<>();
    private final Random random = new Random();

    private SharedPreferences prefs;
    private FrameLayout root;
    private LinearLayout launcherView;
    private FrameLayout playerView;
    private VLCVideoLayout videoLayout;
    private ListView playlistListView;
    private ArrayAdapter<String> playlistAdapter;
    private TextView statusLabel;
    private Spinner repeatSpinner;
    private Spinner hardwareSpinner;
    private CheckBox shuffleCheckBox;
    private CheckBox mutedCheckBox;
    private NumberPicker cachePicker;

    private LibVLC libVlc;
    private MediaPlayer mediaPlayer;
    private int orderIndex;
    private boolean playerVisible;
    private boolean paused;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        prefs = getSharedPreferences(PrefsName, MODE_PRIVATE);
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        loadSettings();
        buildUi();
        showLauncher();
    }

    @Override
    protected void onDestroy() {
        releasePlayer();
        super.onDestroy();
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        if (!playerVisible || event.getAction() != KeyEvent.ACTION_UP) {
            return super.dispatchKeyEvent(event);
        }

        int keyCode = event.getKeyCode();
        if (keyCode == KeyEvent.KEYCODE_SPACE) {
            togglePause();
            return true;
        }

        if (keyCode == KeyEvent.KEYCODE_BACK ||
            keyCode == KeyEvent.KEYCODE_ESCAPE ||
            keyCode == KeyEvent.KEYCODE_F11) {
            showLauncher();
            return true;
        }

        return true;
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode != RequestVideos || resultCode != RESULT_OK || data == null) {
            return;
        }

        int flags = data.getFlags() & (Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
        Set<String> existing = new HashSet<>(playlist);
        int added = 0;

        ClipData clipData = data.getClipData();
        if (clipData != null) {
            for (int i = 0; i < clipData.getItemCount(); i++) {
                Uri uri = clipData.getItemAt(i).getUri();
                added += addVideoUri(uri, flags, existing);
            }
        } else if (data.getData() != null) {
            added += addVideoUri(data.getData(), flags, existing);
        }

        saveSettings();
        refreshPlaylist();
        toast(added + " video eklendi.");
    }

    private int addVideoUri(Uri uri, int flags, Set<String> existing) {
        if (uri == null) {
            return 0;
        }

        try {
            getContentResolver().takePersistableUriPermission(uri, flags & Intent.FLAG_GRANT_READ_URI_PERMISSION);
        } catch (RuntimeException ignored) {
            // Some providers do not grant persistable permissions.
        }

        String value = uri.toString();
        if (!existing.add(value)) {
            return 0;
        }

        playlist.add(value);
        return 1;
    }

    private void buildUi() {
        root = new FrameLayout(this);
        root.setBackgroundColor(Color.rgb(12, 15, 22));
        setContentView(root);

        launcherView = new LinearLayout(this);
        launcherView.setOrientation(LinearLayout.VERTICAL);
        launcherView.setPadding(dp(20), dp(18), dp(20), dp(18));
        launcherView.setBackgroundColor(Color.rgb(17, 20, 28));
        root.addView(launcherView, new FrameLayout.LayoutParams(-1, -1));

        buildHeader();
        buildLauncherContent();

        playerView = new FrameLayout(this);
        playerView.setBackgroundColor(Color.BLACK);
        videoLayout = new VLCVideoLayout(this);
        playerView.addView(videoLayout, new FrameLayout.LayoutParams(-1, -1));
        root.addView(playerView, new FrameLayout.LayoutParams(-1, -1));
        playerView.setVisibility(View.GONE);
    }

    private void buildHeader() {
        LinearLayout header = new LinearLayout(this);
        header.setGravity(Gravity.CENTER_VERTICAL);
        header.setOrientation(LinearLayout.HORIZONTAL);
        header.setPadding(0, 0, 0, dp(18));
        launcherView.addView(header, new LinearLayout.LayoutParams(-1, dp(112)));

        ImageView logo = new ImageView(this);
        logo.setImageResource(getResources().getIdentifier("brand_logo", "drawable", getPackageName()));
        logo.setScaleType(ImageView.ScaleType.FIT_CENTER);
        header.addView(logo, new LinearLayout.LayoutParams(dp(92), dp(92)));

        LinearLayout titleArea = new LinearLayout(this);
        titleArea.setOrientation(LinearLayout.VERTICAL);
        titleArea.setGravity(Gravity.CENTER_VERTICAL);
        titleArea.setPadding(dp(16), 0, dp(16), 0);
        header.addView(titleArea, new LinearLayout.LayoutParams(0, -1, 1));

        TextView title = text("VideoWallPlayer", 28, Color.WHITE, true);
        titleArea.addView(title);
        TextView subtitle = text("Kenar, kontrol cubugu ve metin olmadan tam ekran video wall oynatma", 14, Color.rgb(161, 237, 240), false);
        titleArea.addView(subtitle);

        ImageView model = new ImageView(this);
        model.setImageResource(getResources().getIdentifier("brand_model", "drawable", getPackageName()));
        model.setScaleType(ImageView.ScaleType.FIT_CENTER);
        header.addView(model, new LinearLayout.LayoutParams(dp(104), dp(104)));
    }

    private void buildLauncherContent() {
        LinearLayout columns = new LinearLayout(this);
        columns.setOrientation(LinearLayout.HORIZONTAL);
        launcherView.addView(columns, new LinearLayout.LayoutParams(-1, 0, 1));

        LinearLayout playlistPanel = panel();
        columns.addView(playlistPanel, new LinearLayout.LayoutParams(0, -1, 1.55f));

        Space(columns, 16, 1);

        ScrollView settingsScroll = new ScrollView(this);
        LinearLayout settingsPanel = panel();
        settingsScroll.addView(settingsPanel);
        columns.addView(settingsScroll, new LinearLayout.LayoutParams(0, -1, 1));

        playlistPanel.addView(text("Oynatma Listesi", 22, Color.WHITE, true));

        playlistAdapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, displayPlaylist());
        playlistListView = new ListView(this);
        playlistListView.setAdapter(playlistAdapter);
        playlistListView.setBackgroundColor(Color.rgb(8, 11, 17));
        playlistListView.setDivider(null);
        playlistListView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
        playlistPanel.addView(playlistListView, new LinearLayout.LayoutParams(-1, 0, 1));

        LinearLayout playlistButtons = new LinearLayout(this);
        playlistButtons.setOrientation(LinearLayout.HORIZONTAL);
        playlistButtons.setPadding(0, dp(12), 0, 0);
        playlistPanel.addView(playlistButtons);

        playlistButtons.addView(button("Video Ekle", v -> selectVideos(), false));
        Space(playlistButtons, 8, 0);
        playlistButtons.addView(button("Secileni Sil", v -> removeSelected(), false));
        Space(playlistButtons, 8, 0);
        playlistButtons.addView(button("Temizle", v -> clearPlaylist(), false));

        statusLabel = text("", 13, Color.rgb(172, 187, 205), false);
        statusLabel.setPadding(0, dp(12), 0, 0);
        playlistPanel.addView(statusLabel);

        settingsPanel.addView(text("Ayarlar", 22, Color.WHITE, true));

        LinearLayout actionRow = new LinearLayout(this);
        actionRow.setOrientation(LinearLayout.HORIZONTAL);
        actionRow.setPadding(0, dp(10), 0, dp(18));
        settingsPanel.addView(actionRow);
        actionRow.addView(button("Videoyu Baslat", v -> startPlayback(), true));
        Space(actionRow, 8, 0);
        actionRow.addView(button("Ayar Kaydet", v -> {
            saveSettings();
            toast("Ayarlar kaydedildi.");
        }, false));

        repeatSpinner = spinner(new String[] {"Liste bitince basa don", "Ayni videoyu tekrarla", "Liste bitince dur"});
        hardwareSpinner = spinner(new String[] {"Otomatik", "Kapali"});
        shuffleCheckBox = checkbox("Karisik oynat");
        mutedCheckBox = checkbox("Sessiz");
        cachePicker = new NumberPicker(this);
        cachePicker.setMinValue(100);
        cachePicker.setMaxValue(10000);
        cachePicker.setValue(Math.max(100, prefs.getInt(KeyCache, 3000)));

        settingsPanel.addView(field("Tekrar modu", repeatSpinner));
        settingsPanel.addView(shuffleCheckBox);
        settingsPanel.addView(mutedCheckBox);
        settingsPanel.addView(field("Donanim hizlandirma", hardwareSpinner));
        settingsPanel.addView(field("Onbellek (ms)", cachePicker));
        settingsPanel.addView(text("Video ekraninda Space duraklatir/devam ettirir. Geri, Esc veya F11 oynatimdan cikar.", 13, Color.rgb(172, 187, 205), false));

        repeatSpinner.setSelection(prefs.getInt(KeyRepeat, RepeatAll));
        hardwareSpinner.setSelection(prefs.getInt(KeyHardware, 0));
        shuffleCheckBox.setChecked(prefs.getBoolean(KeyShuffle, false));
        mutedCheckBox.setChecked(prefs.getBoolean(KeyMuted, false));
        refreshPlaylist();
    }

    private LinearLayout panel() {
        LinearLayout panel = new LinearLayout(this);
        panel.setOrientation(LinearLayout.VERTICAL);
        panel.setPadding(dp(18), dp(16), dp(18), dp(16));
        panel.setBackgroundColor(Color.rgb(25, 30, 40));
        return panel;
    }

    private View field(String label, View editor) {
        LinearLayout box = new LinearLayout(this);
        box.setOrientation(LinearLayout.VERTICAL);
        box.setPadding(0, dp(12), 0, dp(8));
        box.addView(text(label, 13, Color.rgb(172, 187, 205), false));
        box.addView(editor, new LinearLayout.LayoutParams(-1, -2));
        return box;
    }

    private Spinner spinner(String[] values) {
        Spinner spinner = new Spinner(this);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, values);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinner.setAdapter(adapter);
        return spinner;
    }

    private CheckBox checkbox(String label) {
        CheckBox checkBox = new CheckBox(this);
        checkBox.setText(label);
        checkBox.setTextColor(Color.WHITE);
        return checkBox;
    }

    private Button button(String label, View.OnClickListener listener, boolean primary) {
        Button button = new Button(this);
        button.setText(label);
        button.setAllCaps(false);
        button.setTextColor(primary ? Color.rgb(4, 19, 27) : Color.WHITE);
        button.setBackgroundColor(primary ? Color.rgb(0, 202, 218) : Color.rgb(38, 48, 64));
        button.setMinHeight(dp(36));
        button.setMinimumHeight(dp(36));
        button.setPadding(dp(12), 0, dp(12), 0);
        button.setOnClickListener(listener);
        return button;
    }

    private TextView text(String value, int sp, int color, boolean bold) {
        TextView textView = new TextView(this);
        textView.setText(value);
        textView.setTextColor(color);
        textView.setTextSize(sp);
        if (bold) {
            textView.setTypeface(android.graphics.Typeface.DEFAULT_BOLD);
        }
        return textView;
    }

    private void Space(LinearLayout parent, int sizeDp, int weight) {
        View spacer = new View(this);
        LinearLayout.LayoutParams params = parent.getOrientation() == LinearLayout.HORIZONTAL
            ? new LinearLayout.LayoutParams(dp(sizeDp), weight == 0 ? 1 : 0)
            : new LinearLayout.LayoutParams(1, dp(sizeDp));
        parent.addView(spacer, params);
    }

    private void selectVideos() {
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("video/*");
        intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true);
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION);
        startActivityForResult(intent, RequestVideos);
    }

    private void removeSelected() {
        int position = playlistListView.getCheckedItemPosition();
        if (position == AdapterView.INVALID_POSITION || position >= playlist.size()) {
            return;
        }

        playlist.remove(position);
        saveSettings();
        refreshPlaylist();
    }

    private void clearPlaylist() {
        playlist.clear();
        saveSettings();
        refreshPlaylist();
    }

    private void startPlayback() {
        saveSettings();
        if (playlist.isEmpty()) {
            toast("Oynatma listesi bos.");
            return;
        }

        buildPlayOrder();
        orderIndex = 0;
        showPlayer();
        playCurrent();
    }

    private void buildPlayOrder() {
        playOrder.clear();
        for (int i = 0; i < playlist.size(); i++) {
            playOrder.add(i);
        }
        if (shuffleCheckBox.isChecked() && playOrder.size() > 1) {
            Collections.shuffle(playOrder, random);
        }
    }

    private void showPlayer() {
        launcherView.setVisibility(View.GONE);
        playerView.setVisibility(View.VISIBLE);
        playerVisible = true;
        hideSystemUi();
        ensurePlayer();
    }

    private void showLauncher() {
        playerVisible = false;
        paused = false;
        releasePlayer();
        playerView.setVisibility(View.GONE);
        launcherView.setVisibility(View.VISIBLE);
        showSystemUi();
    }

    private void ensurePlayer() {
        if (libVlc != null && mediaPlayer != null) {
            return;
        }

        ArrayList<String> options = new ArrayList<>();
        options.add("--no-video-title-show");
        options.add("--no-osd");
        options.add("--quiet");
        options.add("--file-caching=" + cachePicker.getValue());
        options.add("--network-caching=" + cachePicker.getValue());
        options.add("--avcodec-hw=" + (hardwareSpinner.getSelectedItemPosition() == 1 ? "none" : "any"));

        libVlc = new LibVLC(this, options);
        mediaPlayer = new MediaPlayer(libVlc);
        mediaPlayer.attachViews(videoLayout, null, false, false);
        mediaPlayer.setEventListener(event -> {
            if (event.type == MediaPlayer.Event.EndReached || event.type == MediaPlayer.Event.EncounteredError) {
                runOnUiThread(this::playNextAfterEnd);
            }
        });
        mediaPlayer.setVolume(mutedCheckBox.isChecked() ? 0 : 100);
    }

    private void playCurrent() {
        if (mediaPlayer == null || playOrder.isEmpty() || orderIndex < 0 || orderIndex >= playOrder.size()) {
            return;
        }

        Uri uri = Uri.parse(playlist.get(playOrder.get(orderIndex)));
        Media media = new Media(libVlc, uri);
        boolean hardware = hardwareSpinner.getSelectedItemPosition() != 1;
        media.setHWDecoderEnabled(hardware, false);
        media.addOption(":file-caching=" + cachePicker.getValue());
        media.addOption(":network-caching=" + cachePicker.getValue());
        mediaPlayer.setMedia(media);
        media.release();
        mediaPlayer.play();
        paused = false;
        hideSystemUi();
    }

    private void playNextAfterEnd() {
        if (!playerVisible) {
            return;
        }

        if (repeatSpinner.getSelectedItemPosition() == RepeatOne) {
            playCurrent();
            return;
        }

        orderIndex++;
        if (orderIndex < playOrder.size()) {
            playCurrent();
            return;
        }

        if (repeatSpinner.getSelectedItemPosition() == RepeatNone) {
            showLauncher();
            return;
        }

        buildPlayOrder();
        orderIndex = 0;
        playCurrent();
    }

    private void togglePause() {
        if (mediaPlayer == null || !playerVisible) {
            return;
        }
        paused = !paused;
        if (paused) {
            mediaPlayer.pause();
        } else {
            mediaPlayer.play();
        }
        hideSystemUi();
    }

    private void releasePlayer() {
        if (mediaPlayer != null) {
            mediaPlayer.stop();
            mediaPlayer.detachViews();
            mediaPlayer.release();
            mediaPlayer = null;
        }

        if (libVlc != null) {
            libVlc.release();
            libVlc = null;
        }
    }

    private void hideSystemUi() {
        getWindow().getDecorView().setSystemUiVisibility(
            View.SYSTEM_UI_FLAG_FULLSCREEN |
            View.SYSTEM_UI_FLAG_HIDE_NAVIGATION |
            View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY |
            View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN |
            View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION |
            View.SYSTEM_UI_FLAG_LAYOUT_STABLE);
        if (android.os.Build.VERSION.SDK_INT >= 30) {
            WindowInsetsController controller = getWindow().getInsetsController();
            if (controller != null) {
                controller.hide(WindowInsets.Type.statusBars() | WindowInsets.Type.navigationBars());
            }
        }
    }

    private void showSystemUi() {
        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_STABLE);
        if (android.os.Build.VERSION.SDK_INT >= 30) {
            WindowInsetsController controller = getWindow().getInsetsController();
            if (controller != null) {
                controller.show(WindowInsets.Type.statusBars() | WindowInsets.Type.navigationBars());
            }
        }
    }

    private void loadSettings() {
        playlist.clear();
        String json = prefs.getString(KeyPlaylist, "[]");
        try {
            JSONArray array = new JSONArray(json);
            for (int i = 0; i < array.length(); i++) {
                playlist.add(array.getString(i));
            }
        } catch (JSONException ignored) {
            playlist.clear();
        }
    }

    private void saveSettings() {
        JSONArray array = new JSONArray();
        for (String item : playlist) {
            array.put(item);
        }

        prefs.edit()
            .putString(KeyPlaylist, array.toString())
            .putInt(KeyRepeat, repeatSpinner == null ? RepeatAll : repeatSpinner.getSelectedItemPosition())
            .putBoolean(KeyShuffle, shuffleCheckBox != null && shuffleCheckBox.isChecked())
            .putBoolean(KeyMuted, mutedCheckBox != null && mutedCheckBox.isChecked())
            .putInt(KeyHardware, hardwareSpinner == null ? 0 : hardwareSpinner.getSelectedItemPosition())
            .putInt(KeyCache, cachePicker == null ? 3000 : cachePicker.getValue())
            .apply();
    }

    private void refreshPlaylist() {
        if (playlistAdapter != null) {
            playlistAdapter.clear();
            playlistAdapter.addAll(displayPlaylist());
            playlistAdapter.notifyDataSetChanged();
        }

        if (statusLabel != null) {
            statusLabel.setText(playlist.size() + " video listede.");
        }
    }

    private List<String> displayPlaylist() {
        ArrayList<String> display = new ArrayList<>();
        for (String value : playlist) {
            Uri uri = Uri.parse(value);
            String label = uri.getLastPathSegment();
            display.add(label == null ? value : label);
        }
        return display;
    }

    private int dp(int value) {
        return Math.round(value * getResources().getDisplayMetrics().density);
    }

    private void toast(String message) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
    }
}
