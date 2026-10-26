using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Util;
using levelListExtension.HarmonyPatches;
using UnityEngine;

namespace levelListExtension.Settings
{
    //color pickers in the gameplay setup panel, so the song list updates while it is on screen
    class ColorsHandler : PersistentSingleton<ColorsHandler>
    {
        [UIValue("rank1")]
        public Color Rank1
        {
            get => ToColor(Configuration.Instance.Rank_1_Color);
            set => SetColor(ref Configuration.Instance.Rank_1_Color, value);
        }

        [UIValue("rank2")]
        public Color Rank2
        {
            get => ToColor(Configuration.Instance.Rank_2_Color);
            set => SetColor(ref Configuration.Instance.Rank_2_Color, value);
        }

        [UIValue("rank3")]
        public Color Rank3
        {
            get => ToColor(Configuration.Instance.Rank_3_Color);
            set => SetColor(ref Configuration.Instance.Rank_3_Color, value);
        }

        [UIValue("rankOther")]
        public Color RankOther
        {
            get => ToColor(Configuration.Instance.Rank_Other_Color);
            set => SetColor(ref Configuration.Instance.Rank_Other_Color, value);
        }

        [UIValue("pp")]
        public Color Pp
        {
            get => ToColor(Configuration.Instance.Pp_Color);
            set => SetColor(ref Configuration.Instance.Pp_Color, value);
        }

        [UIValue("fc")]
        public Color Fc
        {
            get => ToColor(Configuration.Instance.FullCombo_Color);
            set => SetColor(ref Configuration.Instance.FullCombo_Color, value);
        }

        [UIValue("notFc")]
        public Color NotFc
        {
            get => ToColor(Configuration.Instance.NotFullCombo_Color);
            set => SetColor(ref Configuration.Instance.NotFullCombo_Color, value);
        }

        [UIValue("starSs")]
        public Color StarSs
        {
            get => ToColor(Configuration.Instance.Star_ScoreSaber_Color);
            set => SetColor(ref Configuration.Instance.Star_ScoreSaber_Color, value);
        }

        [UIValue("starBl")]
        public Color StarBl
        {
            get => ToColor(Configuration.Instance.Star_BeatLeader_Color);
            set => SetColor(ref Configuration.Instance.Star_BeatLeader_Color, value);
        }

        [UIValue("mapper")]
        public Color Mapper
        {
            get => ToColor(Configuration.Instance.Mapper_Color);
            set => SetColor(ref Configuration.Instance.Mapper_Color, value);
        }

        [UIValue("easy")]
        public Color Easy
        {
            get => ToColor(Configuration.Instance.Difficulty_Easy_Color);
            set => SetColor(ref Configuration.Instance.Difficulty_Easy_Color, value);
        }

        [UIValue("normal")]
        public Color Normal
        {
            get => ToColor(Configuration.Instance.Difficulty_Normal_Color);
            set => SetColor(ref Configuration.Instance.Difficulty_Normal_Color, value);
        }

        [UIValue("hard")]
        public Color Hard
        {
            get => ToColor(Configuration.Instance.Difficulty_Hard_Color);
            set => SetColor(ref Configuration.Instance.Difficulty_Hard_Color, value);
        }

        [UIValue("expert")]
        public Color Expert
        {
            get => ToColor(Configuration.Instance.Difficulty_Ex_Color);
            set => SetColor(ref Configuration.Instance.Difficulty_Ex_Color, value);
        }

        [UIValue("expertPlus")]
        public Color ExpertPlus
        {
            get => ToColor(Configuration.Instance.Difficulty_ExPlus_Color);
            set => SetColor(ref Configuration.Instance.Difficulty_ExPlus_Color, value);
        }

        [UIValue("accSSS")]
        public Color AccSSS
        {
            get => ToColor(Configuration.Instance.Rank_SSS_Color);
            set => SetColor(ref Configuration.Instance.Rank_SSS_Color, value);
        }

        [UIValue("accSSPlus")]
        public Color AccSSPlus
        {
            get => ToColor(Configuration.Instance.Rank_SSPlus_Color);
            set => SetColor(ref Configuration.Instance.Rank_SSPlus_Color, value);
        }

        [UIValue("accSS")]
        public Color AccSS
        {
            get => ToColor(Configuration.Instance.Rank_SS_Color);
            set => SetColor(ref Configuration.Instance.Rank_SS_Color, value);
        }

        [UIValue("accSPlus")]
        public Color AccSPlus
        {
            get => ToColor(Configuration.Instance.Rank_SPlus_Color);
            set => SetColor(ref Configuration.Instance.Rank_SPlus_Color, value);
        }

        [UIValue("accS")]
        public Color AccS
        {
            get => ToColor(Configuration.Instance.Rank_S_Color);
            set => SetColor(ref Configuration.Instance.Rank_S_Color, value);
        }

        [UIValue("accA")]
        public Color AccA
        {
            get => ToColor(Configuration.Instance.Rank_A_Color);
            set => SetColor(ref Configuration.Instance.Rank_A_Color, value);
        }

        [UIValue("accB")]
        public Color AccB
        {
            get => ToColor(Configuration.Instance.Rank_B_Color);
            set => SetColor(ref Configuration.Instance.Rank_B_Color, value);
        }

        [UIValue("accC")]
        public Color AccC
        {
            get => ToColor(Configuration.Instance.Rank_C_Color);
            set => SetColor(ref Configuration.Instance.Rank_C_Color, value);
        }

        [UIValue("accD")]
        public Color AccD
        {
            get => ToColor(Configuration.Instance.Rank_D_Color);
            set => SetColor(ref Configuration.Instance.Rank_D_Color, value);
        }

        [UIValue("accE")]
        public Color AccE
        {
            get => ToColor(Configuration.Instance.Rank_E_Color);
            set => SetColor(ref Configuration.Instance.Rank_E_Color, value);
        }

        [UIValue("accBelow")]
        public Color AccBelow
        {
            get => ToColor(Configuration.Instance.Rank_Below_Color);
            set => SetColor(ref Configuration.Instance.Rank_Below_Color, value);
        }

        //accuracy colors are picked by threshold, so label them with the threshold instead of the rank letter
        [UIValue("accLabelSSS")]
        private string AccLabelSSS => AccLabel(Configuration.Instance.Rank_SSS);

        [UIValue("accLabelSSPlus")]
        private string AccLabelSSPlus => AccLabel(Configuration.Instance.Rank_SSPlus);

        [UIValue("accLabelSS")]
        private string AccLabelSS => AccLabel(Configuration.Instance.Rank_SS);

        [UIValue("accLabelSPlus")]
        private string AccLabelSPlus => AccLabel(Configuration.Instance.Rank_SPlus);

        [UIValue("accLabelS")]
        private string AccLabelS => AccLabel(Configuration.Instance.Rank_S);

        [UIValue("accLabelA")]
        private string AccLabelA => AccLabel(Configuration.Instance.Rank_A);

        [UIValue("accLabelB")]
        private string AccLabelB => AccLabel(Configuration.Instance.Rank_B);

        [UIValue("accLabelC")]
        private string AccLabelC => AccLabel(Configuration.Instance.Rank_C);

        [UIValue("accLabelD")]
        private string AccLabelD => AccLabel(Configuration.Instance.Rank_D);

        [UIValue("accLabelE")]
        private string AccLabelE => AccLabel(Configuration.Instance.Rank_E);

        [UIValue("accLabelBelow")]
        private string AccLabelBelow => $"Acc under {Configuration.Instance.Rank_E:0.##}%";

        private static string AccLabel(float threshold)
        {
            return $"Acc {threshold:0.##}%+";
        }

        private static Color ToColor(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
        }

        private static void SetColor(ref string field, Color color)
        {
            field = "#" + ColorUtility.ToHtmlStringRGB(color);
            Configuration.Instance.Changed();
            LevelList.RefreshVisibleCells();
        }
    }
}
