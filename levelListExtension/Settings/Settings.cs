using System.Collections.Generic;
using System.Linq;

namespace levelListExtension.Settings
{
    public class Configuration
    {
        public static Configuration Instance { get; set; } = null;
        public virtual bool enable { get; set; } = true;
        public virtual bool refresh { get; set; } = true;
        public virtual bool priorityPlaylist { get; set; } = true;

        public List<object> options = new object[] { "Score Saber", "Beat Leader"}.ToList();

        public string listChoice = "Score Saber";


        public int selectDiff =4;
        public float Rank_SSS = 99.5F;
        public float Rank_SSPlus = 99.0F;
        public float Rank_SS = 98.5F;
        public float Rank_SPlus = 98.0F;
        public float Rank_S = 97.5F;
        public float Rank_A = 97.0F;
        public float Rank_B = 96.5F;
        public float Rank_C = 96.0F;
        public float Rank_D = 95.0F;
        public float Rank_E = 94.0F;

        public string Rank_SSS_Color = "#00FFFF";
        public string Rank_SSPlus_Color = "#00FFFF";
        public string Rank_SS_Color = "#00FFFF";
        public string Rank_SPlus_Color = "#00FF00";
        public string Rank_S_Color = "#00FF00";
        public string Rank_A_Color = "#00FF00";
        public string Rank_B_Color = "#FFFF00";
        public string Rank_C_Color = "#FFFF00";
        public string Rank_D_Color = "#FFFF00";
        public string Rank_E_Color = "#FFFF00";
        //accuracy under Rank_E
        public string Rank_Below_Color = "#FF0000";

        public string Difficulty_ExPlus_Color = "#AC66FF";
        public string Difficulty_Ex_Color = "#FF3B27";
        public string Difficulty_Hard_Color  = "#FF7300";
        public string Difficulty_Normal_Color = "#00FFFF";
        public string Difficulty_Easy_Color = "#00FF00";

        public string Rank_1_Color = "#FFD700";
        public string Rank_2_Color = "#C0C0C0";
        public string Rank_3_Color = "#CD7F32";
        public string Rank_Other_Color = "#FFFFFF";
        public string Pp_Color = "#00FF00";
        public string FullCombo_Color = "#FFD700";
        public string NotFullCombo_Color = "#FF0000";
        public string Star_ScoreSaber_Color = "#FFCC4E";
        public string Star_BeatLeader_Color = "#FF00FF";
        public string Mapper_Color = "#00FF00";

        //BSIPA overrides this to save the file, the color fields above are not tracked on their own
        public virtual void Changed() { }
    }
}
