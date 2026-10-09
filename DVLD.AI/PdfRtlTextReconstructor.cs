using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Content;

namespace DVLD.AI
{
    public static class PdfRtlTextReconstructor
    {
        private enum GlyphDirection
        {
            Neutral,
            Rtl,
            Ltr
        }


        private sealed class GlyphInfo
        {
            public string Text { get; set; } =
                string.Empty;

            public double X { get; set; }

            public double Y { get; set; }

            public double Width { get; set; }

            public double FontSize { get; set; }

            public double CenterX =>
                X + (Width / 2.0);

            public GlyphDirection Direction { get; set; }
        }


        private sealed class LineInfo
        {
            public double Y { get; set; }

            public List<GlyphInfo> Glyphs { get; } =
                new();
        }


        private sealed class DirectionRun
        {
            public GlyphDirection Direction { get; set; }

            public List<GlyphInfo> Glyphs { get; } =
                new();
        }


        public static string Reconstruct(
            Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(
                    nameof(page));
            }


            List<GlyphInfo> glyphs =
                page.Letters
                    .Select(letter =>
                    {
                        string value =
                            NormalizeGlyphText(
                                letter.Value);

                        return new GlyphInfo
                        {
                            Text =
                                value,

                            X =
                                letter.Location.X,

                            Y =
                                letter.Location.Y,

                            Width =
                                Math.Abs(
                                    letter.Width),

                            FontSize =
                                Math.Abs(
                                    letter.FontSize),

                            Direction =
                                ClassifyDirection(
                                    value)
                        };
                    })
                    .Where(glyph =>
                        !string.IsNullOrEmpty(
                            glyph.Text))
                    .ToList();


            if (glyphs.Count == 0)
            {
                return string.Empty;
            }


            double lineTolerance =
                CalculateLineTolerance(
                    glyphs);


            List<LineInfo> lines =
                GroupIntoLines(
                    glyphs,
                    lineTolerance);


            StringBuilder result =
                new StringBuilder();


            foreach (LineInfo line
                     in lines
                         .OrderByDescending(
                             line => line.Y))
            {
                string lineText =
                    ReconstructLine(
                        line);


                if (string.IsNullOrWhiteSpace(
                        lineText))
                {
                    continue;
                }


                result.AppendLine(
                    lineText);
            }


            return NormalizeFinalText(
                result.ToString());
        }


        private static List<LineInfo> GroupIntoLines(
            List<GlyphInfo> glyphs,
            double tolerance)
        {
            List<LineInfo> lines =
                new List<LineInfo>();


            foreach (GlyphInfo glyph
                     in glyphs
                         .OrderByDescending(
                             glyph => glyph.Y))
            {
                LineInfo nearestLine =
                    lines
                        .Where(line =>
                            Math.Abs(
                                line.Y -
                                glyph.Y) <=
                            tolerance)
                        .OrderBy(line =>
                            Math.Abs(
                                line.Y -
                                glyph.Y))
                        .FirstOrDefault();


                if (nearestLine == null)
                {
                    nearestLine =
                        new LineInfo
                        {
                            Y =
                                glyph.Y
                        };

                    lines.Add(
                        nearestLine);
                }


                nearestLine.Glyphs.Add(
                    glyph);


                nearestLine.Y =
                    nearestLine.Glyphs
                        .Average(item =>
                            item.Y);
            }


            return lines;
        }


        private static string ReconstructLine(
            LineInfo line)
        {
            List<GlyphInfo> visualOrder =
                line.Glyphs
                    .OrderBy(glyph =>
                        glyph.CenterX)
                    .ToList();


            if (visualOrder.Count == 0)
            {
                return string.Empty;
            }


            GlyphDirection dominantDirection =
                GetDominantDirection(
                    visualOrder);


            /*
             * الرموز والمسافات لا نربطها بأقرب اتجاه فقط.
             * نحلل السياق حتى تبقى المقاطع الإنجليزية
             * والأرقام والمعادلات كـ LTR islands داخل
             * السطر العربي.
             */
            ResolveNeutralDirections(
                visualOrder,
                dominantDirection);


            List<DirectionRun> runs =
                BuildDirectionalRuns(
                    visualOrder);


            if (dominantDirection ==
                GlyphDirection.Rtl)
            {
                runs.Reverse();
            }


            List<string> renderedRuns =
                new List<string>();


            foreach (DirectionRun run
                     in runs)
            {
                string rendered =
                    RenderRun(
                        run);


                if (!string.IsNullOrWhiteSpace(
                        rendered))
                {
                    renderedRuns.Add(
                        rendered.Trim());
                }
            }


            string lineText =
                JoinRuns(
                    renderedRuns);


            return NormalizeLine(
                lineText);
        }


        private static string RenderRun(

            DirectionRun run)
        {
            IEnumerable<GlyphInfo> orderedGlyphs;


            if (run.Direction ==
                GlyphDirection.Rtl)
            {
                orderedGlyphs =
                    run.Glyphs
                        .OrderByDescending(
                            glyph =>
                                glyph.CenterX);
            }
            else
            {
                orderedGlyphs =
                    run.Glyphs
                        .OrderBy(
                            glyph =>
                                glyph.CenterX);
            }


            StringBuilder builder =
                new StringBuilder();


            foreach (GlyphInfo glyph
                     in orderedGlyphs)
            {
                builder.Append(
                    glyph.Text);
            }


            string text =
                builder.ToString();


            /*
             * بعض ملفات RTL تخزن الأقواس
             * المحيطة بمقطع إنجليزي بصورة
             * معكوسة بصرياً:
             *
             * )End Work Area(
             *
             * نحولها إلى:
             *
             * (End Work Area)
             */
            if (run.Direction ==
                GlyphDirection.Ltr)
            {
                text =
                    FixMirroredLatinBrackets(
                        text);
            }


            return text;
        }


        private static List<DirectionRun>
            BuildDirectionalRuns(
                List<GlyphInfo> glyphs)
        {
            List<DirectionRun> runs =
                new List<DirectionRun>();


            DirectionRun currentRun = null;


            foreach (GlyphInfo glyph
                     in glyphs)
            {
                if (currentRun == null ||
                    currentRun.Direction !=
                    glyph.Direction)
                {
                    currentRun =
                        new DirectionRun
                        {
                            Direction =
                                glyph.Direction
                        };


                    runs.Add(
                        currentRun);
                }


                currentRun.Glyphs.Add(
                    glyph);
            }


            return runs;
        }


        private static void ResolveNeutralDirections(
            List<GlyphInfo> glyphs,
            GlyphDirection dominantDirection)
        {
            for (int i = 0;
                 i < glyphs.Count;
                 i++)
            {
                GlyphInfo glyph =
                    glyphs[i];


                if (glyph.Direction !=
                    GlyphDirection.Neutral)
                {
                    continue;
                }


                GlyphDirection leftDirection =
                    FindStrongDirection(
                        glyphs,
                        i,
                        -1);


                GlyphDirection rightDirection =
                    FindStrongDirection(
                        glyphs,
                        i,
                        1);


                string value =
                    glyph.Text;


                /*
                 * المسافة بين محرفين من نفس الاتجاه
                 * تبقى ضمن نفس الـ run.
                 */
                if (IsWhitespaceOnly(
                        value))
                {
                    if (leftDirection !=
                            GlyphDirection.Neutral &&
                        leftDirection ==
                            rightDirection)
                    {
                        glyph.Direction =
                            leftDirection;
                    }
                    else
                    {
                        glyph.Direction =
                            dominantDirection;
                    }


                    continue;
                }


                /*
                 * النقاط والعمليات الرياضية وعلامات
                 * الأرقام تبقى LTR عندما تكون ملاصقة
                 * لمحتوى لاتيني/رقمي.
                 *
                 * أمثلة:
                 * 2.5
                 * v = 2.5
                 * d + db
                 */
                if (IsLtrConnector(
                        value))
                {
                    if (leftDirection ==
                            GlyphDirection.Ltr ||
                        rightDirection ==
                            GlyphDirection.Ltr)
                    {
                        glyph.Direction =
                            GlyphDirection.Ltr;
                    }
                    else
                    {
                        glyph.Direction =
                            dominantDirection;
                    }


                    continue;
                }


                /*
                 * الأقواس التي تحيط بمقطع لاتيني
                 * تدخل معه حتى لا تنفصل عنه عند
                 * قلب ترتيب الـ runs في السطر العربي.
                 */
                if (IsBracket(
                        value))
                {
                    if (leftDirection ==
                            GlyphDirection.Ltr ||
                        rightDirection ==
                            GlyphDirection.Ltr)
                    {
                        glyph.Direction =
                            GlyphDirection.Ltr;
                    }
                    else
                    {
                        glyph.Direction =
                            dominantDirection;
                    }


                    continue;
                }


                if (leftDirection !=
                        GlyphDirection.Neutral &&
                    leftDirection ==
                        rightDirection)
                {
                    glyph.Direction =
                        leftDirection;

                    continue;
                }


                /*
                 * عند حدود RTL/LTR نربط علامات
                 * الترقيم العامة باتجاه السطر الأساسي
                 * بدل استخدام أقرب طرف بشكل عشوائي.
                 */
                glyph.Direction =
                    dominantDirection;
            }
        }


        private static GlyphDirection
            FindStrongDirection(

                List<GlyphInfo> glyphs,
                int startIndex,
                int step)
        {
            int index =
                startIndex + step;


            while (index >= 0 &&
                   index < glyphs.Count)
            {
                if (glyphs[index].Direction !=
                    GlyphDirection.Neutral)
                {
                    return glyphs[index]
                        .Direction;
                }


                index += step;
            }


            return GlyphDirection.Neutral;
        }


        private static double FindStrongDistance(
            List<GlyphInfo> glyphs,
            int startIndex,
            int step)
        {
            int index =
                startIndex + step;


            while (index >= 0 &&
                   index < glyphs.Count)
            {
                if (glyphs[index].Direction !=
                    GlyphDirection.Neutral)
                {
                    return Math.Abs(
                        glyphs[index].CenterX -
                        glyphs[startIndex]
                            .CenterX);
                }


                index += step;
            }


            return double.MaxValue;
        }


        private static GlyphDirection
            GetDominantDirection(
                List<GlyphInfo> glyphs)
        {
            int rtlCharacters = 0;
            int ltrCharacters = 0;


            foreach (GlyphInfo glyph
                     in glyphs)
            {
                foreach (char character
                         in glyph.Text)
                {
                    if (IsArabicCharacter(
                            character))
                    {
                        rtlCharacters++;
                    }
                    else if (
                        IsLatinCharacter(
                            character) ||
                        char.IsDigit(
                            character))
                    {
                        ltrCharacters++;
                    }
                }
            }


            int strongCharacters =
                rtlCharacters +
                ltrCharacters;


            if (strongCharacters == 0)
            {
                return GlyphDirection.Ltr;
            }


            if (rtlCharacters == 0)
            {
                return GlyphDirection.Ltr;
            }


            if (ltrCharacters == 0)
            {
                return GlyphDirection.Rtl;
            }


            /*
             * السطر العربي قد يحتوي معادلة طويلة
             * أو مصطلحات إنجليزية كثيرة، لذلك
             * لا نشترط أغلبية مطلقة للعربي.
             */
            double rtlRatio =
                (double)rtlCharacters /
                strongCharacters;


            return rtlRatio >= 0.30
                ? GlyphDirection.Rtl
                : GlyphDirection.Ltr;
        }


        private static GlyphDirection
            ClassifyDirection(

                string text)
        {
            int rtlCount = 0;
            int ltrCount = 0;


            foreach (char character
                     in text)
            {
                if (IsArabicCharacter(
                        character))
                {
                    rtlCount++;
                }
                else if (
                    IsLatinCharacter(
                        character) ||
                    char.IsDigit(
                        character))
                {
                    ltrCount++;
                }
            }


            if (rtlCount > ltrCount)
            {
                return GlyphDirection.Rtl;
            }


            if (ltrCount > 0)
            {
                return GlyphDirection.Ltr;
            }


            return GlyphDirection.Neutral;
        }


        private static bool IsArabicCharacter(
            char character)
        {
            return
                (character >= '\u0600' &&
                 character <= '\u06FF') ||

                (character >= '\u0750' &&
                 character <= '\u077F') ||

                (character >= '\u08A0' &&
                 character <= '\u08FF') ||

                (character >= '\uFB50' &&
                 character <= '\uFDFF') ||

                (character >= '\uFE70' &&
                 character <= '\uFEFF');
        }


        private static bool IsLatinCharacter(
            char character)
        {
            return
                (character >= 'A' &&
                 character <= 'Z') ||

                (character >= 'a' &&
                 character <= 'z');
        }


        private static double CalculateLineTolerance(
            List<GlyphInfo> glyphs)
        {
            List<double> fontSizes =
                glyphs
                    .Select(glyph =>
                        glyph.FontSize)
                    .Where(size =>
                        size > 0)
                    .OrderBy(size =>
                        size)
                    .ToList();


            if (fontSizes.Count == 0)
            {
                return 3.0;
            }


            double median =
                fontSizes[
                    fontSizes.Count / 2];


            /*
             * نسمح باختلاف بسيط في baseline
             * بين النص والمعادلات بدون توسيع
             * التجميع لدرجة دمج سطرين مستقلين.
             */
            return Math.Clamp(
                median * 0.40,
                1.75,
                5.0);
        }


        private static string NormalizeGlyphText(

            string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }


            return text
                .Normalize(
                    NormalizationForm.FormKC)
                .Replace(
                    "\u0640",
                    string.Empty)
                .Replace(
                    '\u00A0',
                    ' ')
                .Replace(
                    '\u2007',
                    ' ')
                .Replace(
                    '\u202F',
                    ' ');
        }


        private static string FixMirroredLatinBrackets(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }


            string trimmed =
                text.Trim();


            if (trimmed.Length >= 2)
            {
                if (trimmed[0] == ')' &&
                    trimmed[^1] == '(')
                {
                    trimmed =
                        "(" +
                        trimmed[1..^1] +
                        ")";
                }
                else if (
                    trimmed[0] == ']' &&
                    trimmed[^1] == '[')
                {
                    trimmed =
                        "[" +
                        trimmed[1..^1] +
                        "]";
                }
                else if (
                    trimmed[0] == '}' &&
                    trimmed[^1] == '{')
                {
                    trimmed =
                        "{" +
                        trimmed[1..^1] +
                        "}";
                }
            }


            trimmed =
                Regex.Replace(
                    trimmed,
                    @"\)([^()\r\n]+)\(",
                    "($1)");


            trimmed =
                Regex.Replace(
                    trimmed,
                    @"\]([^\[\]\r\n]+)\[",
                    "[$1]");


            return trimmed;
        }


        private static string JoinRuns(
            List<string> runs)
        {
            if (runs == null ||
                runs.Count == 0)
            {
                return string.Empty;
            }


            StringBuilder builder =
                new StringBuilder();


            foreach (string run
                     in runs)
            {
                string current =
                    run?.Trim() ??
                    string.Empty;


                if (current.Length == 0)
                {
                    continue;
                }


                if (builder.Length > 0 &&
                    NeedsSpaceBetween(
                        builder[^1],
                        current[0]))
                {
                    builder.Append(' ');
                }


                builder.Append(
                    current);
            }


            return builder.ToString();
        }


        private static bool NeedsSpaceBetween(
            char left,
            char right)
        {
            if (left == '(' ||
                left == '[' ||
                left == '{')
            {
                return false;
            }


            if (right == ')' ||
                right == ']' ||
                right == '}' ||
                right == ',' ||
                right == '.' ||
                right == ';' ||
                right == ':' ||
                right == '،' ||
                right == '؛' ||
                right == '?' ||
                right == '؟' ||
                right == '!')
            {
                return false;
            }


            return true;
        }


        private static bool IsWhitespaceOnly(
            string text)
        {
            return
                !string.IsNullOrEmpty(text) &&
                text.All(
                    char.IsWhiteSpace);
        }


        private static bool IsBracket(
            string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }


            return text.All(character =>
                character == '(' ||
                character == ')' ||
                character == '[' ||
                character == ']' ||
                character == '{' ||
                character == '}');
        }


        private static bool IsLtrConnector(
            string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }


            foreach (char character
                     in text)
            {
                if (character == '.' ||
                    character == ',' ||
                    character == ':' ||
                    character == ';' ||
                    character == '+' ||
                    character == '-' ||
                    character == '=' ||
                    character == '/' ||
                    character == '\\' ||
                    character == '*' ||
                    character == '%' ||
                    character == '^' ||
                    character == '<' ||
                    character == '>' ||
                    character == '_' ||
                    character == '#' ||
                    character == '&' ||
                    character == '|' ||
                    character == '~')
                {
                    continue;
                }


                return false;
            }


            return true;
        }


        private static string NormalizeLine(

            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            text =
                Regex.Replace(
                    text,
                    @"[ \t]+",
                    " ");


            text =
                Regex.Replace(
                    text,
                    @"\s+([،,.;؛:!?؟])",
                    "$1");


            text =
                Regex.Replace(
                    text,
                    @"\(\s+",
                    "(");


            text =
                Regex.Replace(
                    text,
                    @"\s+\)",
                    ")");


            text =
                Regex.Replace(
                    text,
                    @"\[\s+",
                    "[");


            text =
                Regex.Replace(
                    text,
                    @"\s+\]",
                    "]");


            return text.Trim();
        }


        private static string NormalizeFinalText(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }


            string[] lines =
                text.Split(
                    new[]
                    {
                        "\r\n",
                        "\r",
                        "\n"
                    },
                    StringSplitOptions
                        .RemoveEmptyEntries);


            return string.Join(
                Environment.NewLine,
                lines
                    .Select(line =>
                        NormalizeLine(
                            line))
                    .Where(line =>
                        !string.IsNullOrWhiteSpace(
                            line)));
        }
    }
}