using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// renders text
    /// </summary>
    public class TextSprite : IExpansion
    {
        /// <summary>
        /// text to render
        /// </summary>
        public string text = "";

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public GameObject parent { get; set; }
        
        private Brush brush;
        Point offset;
        string fontFamily;
        int fontsize;
        private bool disposed;

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public int Priority { get { return 0; } }

        /// <summary>
        /// Creates a new text sprite object
        /// </summary>
        /// <param name="fontFamily">TextSprite.Font string to use as the font name</param>
        /// <param name="fontsize">Pt size of font</param>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public TextSprite(string fontFamily, int fontsize, int r, int g, int b)
        {
            brush = new Brush(r, g, b);
            this.fontFamily = fontFamily;
            this.fontsize = fontsize;
            offset = new Point(0, 0);
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void OnCreate()
        {
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void PreStep()
        {
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void Step()
        {
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onDestroy()
        {
            disposed = true;
            brush.Dispose();
        }
        /// <summary>
        /// Draws the text to the screen
        /// </summary>
        public void onRender()
        {
            if (!disposed)
            {
                var local = Engine.camera.Global2LocalCoords(parent.pos);
                RenderTarget.DrawText(text, fontFamily, fontsize, (int)local.x, (int)local.y, brush);
            }
        }
    }

#pragma warning disable CS1591
    public static class Fonts
    {
        public static string Arial = "Arial";
        public static string Bahnschrift = "Bahnschrift";
        public static string Calibri = "Calibri";
        public static string Cambria = "Cambria";
        public static string CambriaMath = "Cambria Math";
        public static string Candara = "Candara";
        public static string ComicSansMS = "Comic Sans MS";
        public static string Consolas = "Consolas";
        public static string Constantia = "Constantia";
        public static string Corbel = "Corbel";
        public static string CourierNew = "Courier New";
        public static string Ebrima = "Ebrima";
        public static string FranklinGothic = "Franklin Gothic";
        public static string Gabriola = "Gabriola";
        public static string Gadugi = "Gadugi";
        public static string Georgia = "Georgia";
        public static string Impact = "Impact";
        public static string JavaneseText = "Javanese Text";
        public static string LeelawadeeUI = "Leelawadee UI";
        public static string LucidaConsole = "Lucida Console";
        public static string LucidaSansUnicode = "Lucida Sans Unicode";
        public static string MalgunGothic = "Malgun Gothic";
        public static string MicrosoftHimalaya = "Microsoft Himalaya";
        public static string MicrosoftJhengHei = "Microsoft JhengHei";
        public static string MicrosoftJhengHeiUI = "Microsoft JhengHei UI";
        public static string MicrosoftNewTaiLue = "Microsoft New Tai Lue";
        public static string MicrosoftPhagsPa = "Microsoft PhagsPa";
        public static string MicrosoftSansSerif = "Microsoft Sans Serif";
        public static string MicrosoftTaiLe = "Microsoft Tai Le";
        public static string MicrosoftYaHei = "Microsoft YaHei";
        public static string MicrosoftYaHeiUI = "Microsoft YaHei UI";
        public static string MicrosoftYiBaiti = "Microsoft Yi Baiti";
        public static string MingLiU_HKSCSExtB = "MingLiU_HKSCS-ExtB";
        public static string MongolianBaiti = "Mongolian Baiti";
        public static string MSGothic = "MS Gothic";
        public static string MSUIGothic = "MS UI Gothic";
        public static string MSPGothic = "MS PGothic";
        public static string MVBoli = "MV Boli";
        public static string MyanmarText = "Myanmar Text";
        public static string NirmalaUI = "Nirmala UI";
        public static string PalatinoLinotype = "Palatino Linotype";
        public static string SegoeMDL2Assets = "Segoe MDL2 Assets";
        public static string SegoePrint = "Segoe Print";
        public static string SegoeScript = "Segoe Script";
        public static string SegoeUI = "Segoe UI";
        public static string SegoeUIEmoji = "Segoe UI Emoji";
        public static string SegoeUIHistoric = "Segoe UI Historic";
        public static string SegoeUISymbol = "Segoe UI Symbol";
        public static string SimSun = "SimSun";
        public static string NSimSun = "NSimSun";
        public static string SimSunExtB = "SimSun-ExtB";
        public static string SitkaSmall = "Sitka Small";
        public static string SitkaText = "Sitka Text";
        public static string SitkaSubheading = "Sitka Subheading";
        public static string SitkaHeading = "Sitka Heading";
        public static string SitkaDisplay = "Sitka Display";
        public static string SitkaBanner = "Sitka Banner";
        public static string Sylfaen = "Sylfaen";
        public static string Symbol = "Symbol";
        public static string Tahoma = "Tahoma";
        public static string TimesNewRoman = "Times New Roman";
        public static string TrebuchetMS = "Trebuchet MS";
        public static string Verdana = "Verdana";
        public static string Webdings = "Webdings";
        public static string Wingdings = "Wingdings";
        public static string YuGothic = "Yu Gothic";
        public static string YuGothicUI = "Yu Gothic UI";
        public static string HoloLensMDL2Assets = "HoloLens MDL2 Assets";
        public static string MicrosoftOfficePreviewFont = "Microsoft Office Preview Font";
        public static string MSOfficeSymbol = "MS Office Symbol";
        public static string AgencyFB = "Agency FB";
        public static string Algerian = "Algerian";
        public static string BookAntiqua = "Book Antiqua";
        public static string ArialRoundedMT = "Arial Rounded MT";
        public static string BaskervilleOldFace = "Baskerville Old Face";
        public static string Bauhaus93 = "Bauhaus 93";
        public static string BellMT = "Bell MT";
        public static string BernardMT = "Bernard MT";
        public static string BodoniMT = "Bodoni MT";
        public static string BodoniMTPoster = "Bodoni MT Poster";
        public static string BookmanOldStyle = "Bookman Old Style";
        public static string BradleyHandITC = "Bradley Hand ITC";
        public static string Britannic = "Britannic";
        public static string BerlinSansFB = "Berlin Sans FB";
        public static string Broadway = "Broadway";
        public static string BrushScriptMT = "Brush Script MT";
        public static string BookshelfSymbol7 = "Bookshelf Symbol 7";
        public static string CalifornianFB = "Californian FB";
        public static string CalistoMT = "Calisto MT";
        public static string Castellar = "Castellar";
        public static string CenturySchoolbook = "Century Schoolbook";
        public static string Centaur = "Centaur";
        public static string Century = "Century";
        public static string Chiller = "Chiller";
        public static string ColonnaMT = "Colonna MT";
        public static string Cooper = "Cooper";
        public static string CopperplateGothic = "Copperplate Gothic";
        public static string CurlzMT = "Curlz MT";
        public static string Dubai = "Dubai";
        public static string Elephant = "Elephant";
        public static string EngraversMT = "Engravers MT";
        public static string ErasITC = "Eras ITC";
        public static string FelixTitling = "Felix Titling";
        public static string Forte = "Forte";
        public static string FranklinGothicBook = "Franklin Gothic Book";
        public static string FreestyleScript = "Freestyle Script";
        public static string FrenchScriptMT = "French Script MT";
        public static string FootlightMT = "Footlight MT";
        public static string Garamond = "Garamond";
        public static string Gigi = "Gigi";
        public static string GillSansMT = "Gill Sans MT";
        public static string GillSans = "Gill Sans";
        public static string GloucesterMT = "Gloucester MT";
        public static string CenturyGothic = "Century Gothic";
        public static string GoudyOldStyle = "Goudy Old Style";
        public static string GoudyStout = "Goudy Stout";
        public static string HarlowSolid = "Harlow Solid";
        public static string Harrington = "Harrington";
        public static string Haettenschweiler = "Haettenschweiler";
        public static string HighTowerText = "High Tower Text";
        public static string ImprintMTShadow = "Imprint MT Shadow";
        public static string InformalRoman = "Informal Roman";
        public static string BlackadderITC = "Blackadder ITC";
        public static string EdwardianScriptITC = "Edwardian Script ITC";
        public static string KristenITC = "Kristen ITC";
        public static string Jokerman = "Jokerman";
        public static string JuiceITC = "Juice ITC";
        public static string KunstlerScript = "Kunstler Script";
        public static string WideLatin = "Wide Latin";
        public static string LucidaBright = "Lucida Bright";
        public static string LucidaCalligraphy = "Lucida Calligraphy";
        public static string Leelawadee = "Leelawadee";
        public static string LucidaFax = "Lucida Fax";
        public static string LucidaHandwriting = "Lucida Handwriting";
        public static string LucidaSans = "Lucida Sans";
        public static string LucidaSansTypewriter = "Lucida Sans Typewriter";
        public static string Magneto = "Magneto";
        public static string MaiandraGD = "Maiandra GD";
        public static string MaturaMTScriptCapitals = "Matura MT Script Capitals";
        public static string Mistral = "Mistral";
        public static string MicrosoftUighur = "Microsoft Uighur";
        public static string MonotypeCorsiva = "Monotype Corsiva";
        public static string NiagaraEngraved = "Niagara Engraved";
        public static string NiagaraSolid = "Niagara Solid";
        public static string OCRA = "OCR A";
        public static string OldEnglishTextMT = "Old English Text MT";
        public static string Onyx = "Onyx";
        public static string MSOutlook = "MS Outlook";
        public static string PalaceScriptMT = "Palace Script MT";
        public static string Papyrus = "Papyrus";
        public static string Parchment = "Parchment";
        public static string Perpetua = "Perpetua";
        public static string PerpetuaTitlingMT = "Perpetua Titling MT";
        public static string Playbill = "Playbill";
        public static string PoorRichard = "Poor Richard";
        public static string Pristina = "Pristina";
        public static string Rage = "Rage";
        public static string Ravie = "Ravie";
        public static string MSReferenceSansSerif = "MS Reference Sans Serif";
        public static string MSReferenceSpecialty = "MS Reference Specialty";
        public static string Rockwell = "Rockwell";
        public static string ScriptMT = "Script MT";
        public static string ShowcardGothic = "Showcard Gothic";
        public static string SnapITC = "Snap ITC";
        public static string Stencil = "Stencil";
        public static string TwCenMT = "Tw Cen MT";
        public static string TempusSansITC = "Tempus Sans ITC";
        public static string VinerHandITC = "Viner Hand ITC";
        public static string Vivaldi = "Vivaldi";
        public static string VladimirScript = "Vladimir Script";
        public static string Wingdings2 = "Wingdings 2";
        public static string Wingdings3 = "Wingdings 3";
        public static string Roboto = "Roboto";
    }
}
