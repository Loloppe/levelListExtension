using HarmonyLib;
using levelListExtension.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace levelListExtension.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("SetDataFromLevelAsync", MethodType.Normal)]
    internal class LevelList
    {
        private static TextMeshProUGUI packTitle = null;

        public static Dictionary<string, PlayerScore> plScore = new Dictionary<string, PlayerScore>();

        //lookup order: characteristic first, then from the selected difficulty down to Easy
        private static readonly string[] characteristics = { "SoloLawless", "SoloStandard", "SoloOneSaber", "Solo360Degree" };
        private static readonly string[] difficulties = { "Easy", "Normal", "Hard", "Expert", "ExpertPlus" };
        private static readonly string[][] diffKeys = characteristics
            .Select(c => difficulties.Select(d => "_" + d + "_" + c).ToArray())
            .ToArray();

        //vanilla y positions of the cell texts, from the LevelListTableCell prefab (all anchored to the vertical center)
        private const float NameY = 1.2F;
        private const float AuthorY = -1.6F;
        //how far everything moves up to make room for the score line, half of LevelListCellSize.ExtraHeight
        private const float ScoredOffsetY = LevelListCellSize.ExtraHeight / 2;
        private const string ScoreTextName = "levelListExtensionScore";
        //score line column start positions, in percent of the line width
        private const string ColumnRank = "0";
        private const string ColumnAcc = "11";
        private const string ColumnPp = "27.5";
        private const string ColumnMisses = "47";
        //just enough room after FC for one speed modifier, e.g. FC [SF]
        private const string ColumnStars = "63";
        //fixed slot after the widest star rating (★12.34) plus the same gap as the other columns, so the difficulty lines up whatever the stars
        private const string ColumnDiff = "78";
        //only the song speed modifiers are shown
        private static readonly string[] speedModifiers = { "SS", "FS", "SF" };

        private static void Postfix(LevelListTableCell __instance, BeatmapLevel beatmapLevel, bool isFavorite, ref Image ____favoritesBadgeImage,
            ref TextMeshProUGUI ____songNameText, ref TextMeshProUGUI ____songDurationText,
            ref TextMeshProUGUI ____songBpmText, ref TextMeshProUGUI ____songAuthorText)
        {
            var config = Settings.Configuration.Instance;

            //cells are reused, so start from the vanilla layout every time
            var scoreText = GetScoreText(__instance, ____songBpmText);
            SetLayout(scoreText, ____songNameText, ____songDurationText, ____songAuthorText, ____songBpmText, false);

            //get packTitle
            if (packTitle == null)
            {
                var levelsTableView = GameObject.Find("LevelsTableView");
                if (levelsTableView != null)
                {
                    var b = levelsTableView.transform.Find("TableView/Viewport/Content/LevelPackHeaderTableCell/PackName");
                    if (b != null) packTitle = b.GetComponent<TextMeshProUGUI>();
                }
            }
            bool isPlaylist = packTitle && packTitle.text != "Custom Levels";
            //-----------------------------------
            if (!selectUI.instance.IsCreated)
            {
                var resultsView = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().FirstOrDefault();
                selectUI.instance.Create(resultsView);
            }

            //return not custom level or mod disabled
            if (!config.enable || !beatmapLevel.levelID.StartsWith("custom_level", System.StringComparison.Ordinal)) return;

            string levelID = beatmapLevel.levelID.Substring(13);

            //get song highScore
            //set difficulty name
            string diffRaw = null;
            PlayerScore score = null;
            if (config.priorityPlaylist && isPlaylist && Plugin.playlistDiff.TryGetValue(levelID, out diffRaw))
            {
                plScore.TryGetValue(levelID + diffRaw, out score);
            }
            else
            {
                int maxDiff = Mathf.Clamp(config.selectDiff, 0, difficulties.Length - 1);
                foreach (var keys in diffKeys)
                {
                    for (int d = maxDiff; d >= 0; d--)
                    {
                        if (plScore.TryGetValue(levelID + keys[d], out score))
                        {
                            diffRaw = keys[d];
                            goto endSelectDiff;
                        }
                    }
                }
            }
            endSelectDiff:

            if (score != null)
            {
                //setText
                string diff = "";
                string diffColor = "#FFFFFF";
                switch (diffRaw)
                {
                    case "_ExpertPlus_SoloStandard":
                        diff = "Expert+";
                        diffColor = config.Difficulty_ExPlus_Color;
                        break;
                    case "_Expert_SoloStandard":
                        diff = "Expert";
                        diffColor = config.Difficulty_Ex_Color;
                        break;
                    case "_Hard_SoloStandard":
                        diff = "Hard";
                        diffColor = config.Difficulty_Hard_Color;
                        break;
                    case "_Normal_SoloStandard":
                        diff = "Normal";
                        diffColor = config.Difficulty_Normal_Color;
                        break;
                    case "_Easy_SoloStandard":
                        diff = "Easy";
                        diffColor = config.Difficulty_Easy_Color;
                        break;
                }

                float acc = ((float)score.Score.ModifiedScore / score.Leaderboard.MaxScore) * 100;
                string accColor;

                //accuracy color from the rank thresholds
                if (acc >= config.Rank_SSS) accColor = config.Rank_SSS_Color;
                else if (acc >= config.Rank_SSPlus) accColor = config.Rank_SSPlus_Color;
                else if (acc >= config.Rank_SS) accColor = config.Rank_SS_Color;
                else if (acc >= config.Rank_SPlus) accColor = config.Rank_SPlus_Color;
                else if (acc >= config.Rank_S) accColor = config.Rank_S_Color;
                else if (acc >= config.Rank_A) accColor = config.Rank_A_Color;
                else if (acc >= config.Rank_B) accColor = config.Rank_B_Color;
                else if (acc >= config.Rank_C) accColor = config.Rank_C_Color;
                else if (acc >= config.Rank_D) accColor = config.Rank_D_Color;
                else if (acc >= config.Rank_E) accColor = config.Rank_E_Color;
                else accColor = config.Rank_Below_Color;

                //star color
                string starColor = score.isScoreSaber == true ? config.Star_ScoreSaber_Color : config.Star_BeatLeader_Color;

                //build the whole line at once so TMP only re-parses it once
                //each column starts at a fixed fraction of the line width so they line up across rows
                var sb = new System.Text.StringBuilder(256);
                //leaderboard rank first, left empty when the source did not give one
                sb.Append($"<pos={ColumnRank}%>");
                if (score.Score.Rank > 0)
                {
                    //gold, silver and bronze for the podium
                    string rankColor = score.Score.Rank == 1 ? config.Rank_1_Color : score.Score.Rank == 2 ? config.Rank_2_Color : score.Score.Rank == 3 ? config.Rank_3_Color : config.Rank_Other_Color;
                    sb.Append($"<color={rankColor}>#{score.Score.Rank}</color>");
                }
                sb.Append($"<pos={ColumnAcc}%><color={accColor}>{acc.ToString("F2")}%</color>");
                //add pp, then FC or a red X with modifiers after them
                sb.Append($"<pos={ColumnPp}%><color={config.Pp_Color}>{score.Score.Pp.ToString("F2")}</color>pp");
                if (score.Score.FullCombo) sb.Append($"<pos={ColumnMisses}%><color={config.FullCombo_Color}>FC</color>");
                else sb.Append($"<pos={ColumnMisses}%><color={config.NotFullCombo_Color}>X</color>");
                string speed = GetSpeedModifier(score.Score.Modifiers);
                if (speed != null) sb.Append($" [{speed}]");
                //stars and difficulty at the end, with the playlist marker after them
                sb.Append($"<pos={ColumnStars}%><color={starColor}>★</color>{score.Leaderboard.Stars.ToString("F2")}<pos={ColumnDiff}%><color={diffColor}>{diff}</color>");
                if (isPlaylist && Plugin.playlistDiff.ContainsKey(levelID)) sb.Append('*');
                scoreText.text = sb.ToString();

                //artist and mapper share the second line, the score gets its own line below
                //built from the level instead of the current text, since the game skips resetting it when the same level is set twice
                ____songAuthorText.text = $"{beatmapLevel.songAuthorName} [<color={config.Mapper_Color}>{score.Leaderboard.LevelAuthorName}</color>]";
                SetLayout(scoreText, ____songNameText, ____songDurationText, ____songAuthorText, ____songBpmText, true);
            }
        }

        //re-run the postfix on the cells currently on screen
        internal static void RefreshVisibleCells()
        {
            var levelTable = Resources.FindObjectsOfTypeAll<LevelCollectionTableView>().FirstOrDefault();
            if (levelTable == null) return;
            var tableView = Traverse.Create(levelTable).Field("_tableView").GetValue<HMUI.TableView>();
            if (tableView != null && tableView.isActiveAndEnabled) tableView.RefreshCellsContent();
        }

                //modifiers come as a comma separated list like "FS,GN"
        private static string GetSpeedModifier(string modifiers)
        {
            if (string.IsNullOrEmpty(modifiers)) return null;
            foreach (var modifier in modifiers.Split(','))
            {
                string trimmed = modifier.Trim();
                if (System.Array.IndexOf(speedModifiers, trimmed) >= 0) return trimmed;
            }
            return null;
        }

        private static TextMeshProUGUI GetScoreText(LevelListTableCell cell, TextMeshProUGUI songBpmText)
        {
            var existing = cell.transform.Find(ScoreTextName);
            if (existing != null) return existing.GetComponent<TextMeshProUGUI>();

            //clone the bpm text so the new line uses the same font and material
            var scoreText = Object.Instantiate(songBpmText, songBpmText.transform.parent, false);
            scoreText.name = ScoreTextName;
            scoreText.alignment = TextAlignmentOptions.MidlineLeft;
            scoreText.overflowMode = TextOverflowModes.Ellipsis;

            //full width under the author, from the author's left edge to the cell's right edge
            var rect = scoreText.rectTransform;
            var authorRect = cell.transform.Find("SongAuthor") as RectTransform;
            float left = authorRect != null ? authorRect.anchoredPosition.x : 9.28F;
            rect.anchorMin = new Vector2(0F, 0.5F);
            rect.anchorMax = new Vector2(1F, 0.5F);
            rect.pivot = new Vector2(0F, 0.5F);
            //same height as the author text, Ellipsis hides the whole line if it is taller than the rect
            rect.sizeDelta = new Vector2(-left - 2F, authorRect != null ? authorRect.sizeDelta.y : 6F);
            rect.anchoredPosition = new Vector2(left, AuthorY + ScoredOffsetY - LevelListCellSize.ExtraHeight);
            return scoreText;
        }

        private static void SetLayout(TextMeshProUGUI scoreText, TextMeshProUGUI songNameText, TextMeshProUGUI songDurationText,
            TextMeshProUGUI songAuthorText, TextMeshProUGUI songBpmText, bool scored)
        {
            float offset = scored ? ScoredOffsetY : 0F;
            SetY(songNameText.rectTransform, NameY + offset);
            SetY(songDurationText.rectTransform, NameY + offset);
            SetY(songAuthorText.rectTransform, AuthorY + offset);
            SetY(songBpmText.rectTransform, AuthorY + offset);
            var bpmIcon = songBpmText.transform.parent.Find("BpmIcon") as RectTransform;
            if (bpmIcon != null) SetY(bpmIcon, AuthorY + offset);

            scoreText.gameObject.SetActive(scored);
            if (!scored) scoreText.text = "";
        }

        private static void SetY(RectTransform rect, float y)
        {
            var pos = rect.anchoredPosition;
            if (pos.y != y) rect.anchoredPosition = new Vector2(pos.x, y);
        }
    }

    //makes song rows one text line taller so the score fits under the artist/mapper line
    [HarmonyPatch(typeof(LevelCollectionTableView), nameof(LevelCollectionTableView.CellSize))]
    internal class LevelListCellSize
    {
        //same spacing as between the song name and author lines
        public const float ExtraHeight = 2.8F;

        private static void Postfix(int idx, bool ____showLevelPackHeader, ref float __result)
        {
            if (__result <= 0F || !Settings.Configuration.Instance.enable) return;
            if (idx == 0 && ____showLevelPackHeader) return;
            __result += ExtraHeight;
        }
    }
}
