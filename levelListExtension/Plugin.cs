using IPA;
using IPA.Config.Stores;
using System.Collections.Generic;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using Steamworks;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using BeatSaberMarkupLanguage.Settings;
using levelListExtension.Settings;
using HarmonyLib;
using System.Reflection;
using levelListExtension.HarmonyPatches;
using BS_Utils.Utilities;
using System.Threading.Tasks;
using TMPro;
using BeatSaberPlaylistsLib.Types;

namespace levelListExtension
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        private Harmony _harmony;

        [Init]
        public void Init(IPALogger logger,IPA.Config.Config conf)
        {
            Instance = this;
            Log = logger;
            Configuration.Instance = conf.Generated<Configuration>();
            BSMLSettings.instance.AddSettingsMenu("levelListExtension", "levelListExtension.Settings.settings.bsml", SettingsHandler.instance);
        }

        private static int songcount = 0;
        private static int songcountBl = 0;
        private string settingsName = Directory.GetCurrentDirectory() + "\\UserData\\levelListExtension.json";
        private string songFileName = Directory.GetCurrentDirectory() + "\\UserData\\levelListExtension_Songs.json";
        private string songFileNameBl = Directory.GetCurrentDirectory() + "\\UserData\\levelListExtension_SongsBl.json";
        [OnStart]
        public void OnApplicationStart()
        {
            _harmony = new Harmony("com.scifiHerb.BeatSaber.levelListExtension");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            new GameObject("levelListExtensionController").AddComponent<levelListExtensionController>();
            BSEvents.levelSelected += OnLevelSelected;

            //load songs
            string songsText = "";
            LevelList.plScore = null;


            if (File.Exists(songFileName)) songsText = File.ReadAllText(songFileName);
            else songsText = "";
            LevelList.plScore = JsonConvert.DeserializeObject<Dictionary<string, PlayerScore>>(songsText);

            if (LevelList.plScore == null)
                LevelList.plScore = LevelList.plScore = new Dictionary<string, PlayerScore>();

            PlaylistManager.Utilities.Events.playlistSelected += onPlaylistSelected;
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            File.WriteAllText(songFileName, JsonConvert.SerializeObject(LevelList.plScore));
        }

        public static TextMeshProUGUI packTitle = null;
        private void onPlaylistSelected(IPlaylist p, BeatSaberPlaylistsLib.PlaylistManager pm)
        {
            if (pm == null) return;
            string filePath = pm.PlaylistPath + "\\" + p.Filename;

            //load playlist data
            string playlistDataText = "";
            if (File.Exists(filePath + ".bplist")) playlistDataText = File.ReadAllText(filePath + ".bplist");
            else if (File.Exists(filePath + ".json")) playlistDataText = File.ReadAllText(filePath + ".json");
            else playlistDataText = "";

            var playlistData = JsonConvert.DeserializeObject<PlaylistData.Root>(playlistDataText);

            loadPlaylist(playlistData);
        }

        public static Dictionary<string, string> playlistDiff = new Dictionary<string,string>();
        private void loadPlaylist(PlaylistData.Root playlistData)
        {
            if (playlistData == null) { Log.Info("loadPlaylist data null"); return; }
            if (playlistData.songs == null) { Log.Info("loadPlaylist song null"); return; }
            playlistDiff = new Dictionary<string, string>();

            foreach (var song in playlistData.songs)
            {
                if (song.difficulties != null && song.difficulties != null && song.difficulties.Count >= 1)
                {
                    switch (song.difficulties[0].name)
                    {
                        case "easy":
                            song.difficulties[0].name = "Easy";
                            break;
                        case "normal":
                            song.difficulties[0].name = "Normal";
                            break;
                        case "hard":
                            song.difficulties[0].name = "Hard";
                            break;
                        case "expert":
                            song.difficulties[0].name = "Expert";
                            break;
                        case "expertPlus":
                            song.difficulties[0].name = "ExpertPlus";
                            break;
                    }
                    song.difficulties[0].characteristic = "Solo" + song.difficulties[0].characteristic;

                    playlistDiff.Remove(song.hash.ToUpper());
                    playlistDiff.Add(song.hash.ToUpper(), "_" + song.difficulties[0].name + "_" + song.difficulties[0].characteristic);
                }
            }

        }

        private void OnLevelSelected(LevelCollectionViewController lc, BeatmapLevel lv)
        {
            if (!Configuration.Instance.refresh)
            {
                if (Configuration.Instance.listChoice == "Score Saber") GetSongStats(1);
                else if (Configuration.Instance.listChoice == "Beat Leader") GetSongStatsBl(1);
            }
        }
        public static async void GetSongStats(int count, TextMeshProUGUI text = null)
        {
            string url = $"https://scoresaber.com/api/player/{SteamUser.GetSteamID()}";
            var httpClient = new HttpClient();
            if (Configuration.Instance.refresh) songcount = 0;

            for (int i = 0; i < count; i++)
            {
                var response = await httpClient.GetAsync(url + "/scores?sort=recent&page=" + i); // 非同期にWebリクエストを送信する
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync(); // 非同期にレスポンスを読み込む
                    if (result == null) return;

                    var data = JsonConvert.DeserializeObject<PlayerScoresInfo>(result);
                    var limit = count * 8;
                    if (limit > data.Metadata.Total) limit = data.Metadata.Total;

                    foreach (var l in data.PlayerScores)
                    {
                        //priority 
                        string key = l.Leaderboard.SongHash + l.Leaderboard.Difficulty.DifficultyRaw;
                        if (LevelList.plScore.ContainsKey(key))
                        {
                            if (Configuration.Instance.listChoice == "Score Saber" || LevelList.plScore[key].isScoreSaber == true)
                            {
                                LevelList.plScore.Remove(key);
                                LevelList.plScore.Add(key, l);
                                LevelList.plScore[key].isScoreSaber = true;
                            }
                        }
                        else
                        {
                            LevelList.plScore.Remove(key);
                            LevelList.plScore.Add(key, l);
                            LevelList.plScore[key].isScoreSaber = true;
                        }

                        songcount++;

                        if (text != null) text.text = $"({songcount.ToString()}/{(limit).ToString()}) " + $"Loaded <color=#00FF00>{l.Leaderboard.SongName}</color>";

                        if (songcount >= limit)
                        {
                            if (text != null)
                            {
                                text.text += " - Completed";
                                await Task.Delay(2000);
                                text.text = "";
                                return;
                            }
                        }
                    }
                }
                if (i != 0 && i % 30 == 0) await Task.Delay(3000);
            }
        }
        public static async void GetSongStatsBl(int count, TextMeshProUGUI text = null)
        {
        //https://api.beatleader.xyz/player/76561199194622414/scores?sortBy=date&page=1&count=1
            string url = $"https://api.beatleader.xyz/player/{SteamUser.GetSteamID()}";
            var httpClient = new HttpClient();
            if (Configuration.Instance.refresh) songcountBl = 0;

            for (int i = 0; i < count; i++)
            {
                var response = await httpClient.GetAsync(url + $"/scores?sortBy=date&page={i}&count={count*8}"); // 非同期にWebリクエストを送信する
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync(); // 非同期にレスポンスを読み込む
                    if (result == null) return;

                    BL.Root data = null;

                    data = JsonConvert.DeserializeObject<BL.Root>(result);
                    var limit = count * 8;
                    if (limit > data.metadata.total) limit = (int)data.metadata.total;

                    foreach (var l in data.data)
                    {
                        PlayerScore plScore = new PlayerScore();

                        //init
                        plScore.Score = new Score();
                        plScore.Score.Leaderboard = new Leaderboard();
                        plScore.Leaderboard = new Leaderboard();

                        plScore.Leaderboard.SongHash = l.leaderboard.song.hash;
                        plScore.Score.ModifiedScore = (int)l.modifiedScore;
                        plScore.Score.MissedNotes = (int)l.missedNotes;
                        plScore.Score.BadCuts = (int)l.badCuts;
                        if(l.pp!=null)plScore.Score.Pp = (double)l.pp;

                        plScore.Leaderboard.MaxScore = (int)l.leaderboard.difficulty.maxScore;
                        plScore.Leaderboard.LevelAuthorName = l.leaderboard.song.mapper;
                        plScore.Leaderboard.SongAuthorName = l.leaderboard.song.author;
                        plScore.Leaderboard.SongName = l.leaderboard.song.name;
                        plScore.Leaderboard.SongSubName = l.leaderboard.song.subName;
                        if(l.leaderboard.difficulty.stars!=null)plScore.Leaderboard.Stars = (double)l.leaderboard.difficulty.stars;
                        plScore.isScoreSaber = false;
                        plScore.Score.Modifiers = "";

                        string key = $"{l.leaderboard.song.hash.ToUpper()}_{l.leaderboard.difficulty.difficultyName}_Solo{l.leaderboard.difficulty.modeName}";

                        //priority
                        //if plScore.Leaderboard.SongName == null ->Beat Leader
                        if (LevelList.plScore.ContainsKey(key))
                        {
                            if(Configuration.Instance.listChoice == "Beat Leader" || LevelList.plScore[key].isScoreSaber == null || LevelList.plScore[key].isScoreSaber == false)
                            {
                                LevelList.plScore.Remove(key);
                                LevelList.plScore.Add(key, plScore);
                            }
                        }
                        else
                        {
                            LevelList.plScore.Remove(key);
                            LevelList.plScore.Add(key, plScore);
                        }

                        songcountBl++;

                        if (text != null) text.text = $"({songcountBl.ToString()}/{(limit).ToString()}) " + $"Loaded <color=#00FF00>{l.leaderboard.song.name}</color>";
                        //Plugin.Log.Info($"({songcount.ToString()}/{limit.ToString()}) " + $"Loaded <color=#00FF00>{l.Leaderboard.SongName}</color>");

                        if (songcountBl >= limit)
                        {
                            if (text != null)
                            {
                                text.text += " - Completed";
                                await Task.Delay(1000);
                                text.text = "";
                                return;
                            }
                        }
                    }
                }
                if (i != 0 && i % 30 == 0) await Task.Delay(1000);
            }
        }
    }
}
