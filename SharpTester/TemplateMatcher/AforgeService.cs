using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTester
{
    public class AforgeService
    {
        //найденные совпадения
        public TemplateMatch[] _matchings;

        /// <summary>
        /// Количество найденных совпадений
        /// </summary>
        public int CountMatchings
        {
            get => _matchings != null ?  _matchings.Length : 0;
        }


        //ctor
        public AforgeService()
        {

        }

        /// <summary>
        /// Содержит ли исходное изображение представленый образец
        /// </summary>
        /// <param name="pathOriginalImage">путь к файлу исходного изображения</param>
        /// <param name="pathSampleImage">путь к файлу образца</param>
        /// <returns>true если содержит</returns>
        public async Task<bool> IsContains(Bitmap pathOriginalImage, Bitmap pathSampleImage)
        {
            //if (String.IsNullOrEmpty(pathOriginalImage)) throw new ArgumentNullException(nameof(pathOriginalImage));
            //if (String.IsNullOrEmpty(pathSampleImage)) throw new ArgumentNullException(nameof(pathSampleImage));

            //var sample = new Bitmap(pathSampleImage);
            //var orig = new Bitmap(pathOriginalImage);

            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.721f);
            _matchings = await Task.Run(() => tm.ProcessImage(pathOriginalImage, pathSampleImage)
            .OrderBy(x => x.Rectangle.Top).ThenBy(x => x.Rectangle.Right).ToArray()
            )
                ;

            List<Rectangle> groupslist = _matchings.Select(x => x.Rectangle).ToList();
            List<Rectangle> res = null;
            int COUNT1 = groupslist.Count;
            do
            {
                COUNT1 = groupslist.Count;
                groupslist = Fixer(groupslist);
            } while (groupslist.Count < COUNT1);

            for (int K = 0; K < 2; K++)
            {
                groupslist = Fixer(groupslist);
            }

            List<Rectangle> Fixer(List<Rectangle> matches)
            {
                List<Rectangle> groups = new List<Rectangle>();
                foreach (var match in matches)
                {
                    bool found = false;
                    for (int i = 0; i < groups.Count; i++)
                    {
                        var rect = groups[i];
                        var intersection = Rectangle.Intersect(rect, match);
                        if (!intersection.Size.IsEmpty)
                        {
                            found = true;
                            groups[i] = Rectangle.Union(match, rect);
                            break;
                        }
                    }
                    if (!found)
                        groups.Add(match);

                }

                return groups;
            }

            _matchings = groupslist.Select(x => new TemplateMatch(x, 1)).ToArray();

            return _matchings.Any();
        }


        /// <summary>
        /// Получение коллекции найденных мест где находится образец
        /// </summary>
        /// <returns>коллекция найденных мест</returns>
        public List<FoundPlace> GetPlaces()
        {
            List<FoundPlace> result = new List<FoundPlace>();
            if (CountMatchings == 0) return result;

            int id = 0;
            foreach (var match in _matchings)
            {
                FoundPlace place = new FoundPlace
                {
                    Id = ++id,
                    Similarity = match.Similarity,
                    Top = match.Rectangle.Top,
                    Left = match.Rectangle.Left,
                    Height = match.Rectangle.Height,
                    Width = match.Rectangle.Width
                };

                result.Add(place);
            }

            return result;
        }

    }
}
