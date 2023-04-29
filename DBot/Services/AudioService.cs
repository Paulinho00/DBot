using System.Text;
using System.Text.RegularExpressions;

namespace DBot.Services;

public class AudioService
{
    private readonly string[] _supportedFormats = { ".mp3", ".wav" };
    private readonly string _path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Resources\";

    
    /// <summary>
    /// Returns all sounds from Resources directory
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> GetAllSoundsFromLocalFiles()
    {
        

        Dictionary<string, string> categoriesWithSounds = new Dictionary<string, string>();

        string[] categories = Directory.GetDirectories(_path);

        //Adds each category with its sounds to dictionary
        foreach (string category in categories)
        {
            var allSoundsFromCategory = GetAllSoundsFromCategory(category);
            foreach (string parts in allSoundsFromCategory.Keys)
                categoriesWithSounds.Add(parts, allSoundsFromCategory[parts]);


        }
            
            
        //Checks if there are sounds without category and adds them to dictionary
        var filenamesWithoutCategory = GetAllSoundsFromCategory(_path);
        if (filenamesWithoutCategory[filenamesWithoutCategory.Keys.First()].Length != 0)
        {
            foreach(string parts in filenamesWithoutCategory.Keys)
                categoriesWithSounds.Add(parts, filenamesWithoutCategory[parts]);
        }
        return categoriesWithSounds;
    }

    /// <summary>
    /// Get path to a filename within a resources directory
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>Full, absolute path</returns>
    public FileInfo? GetFilePath(String fileName)
    {
        foreach (var format in _supportedFormats)
        {
            try
            {
                var soundFilePath = Directory.GetFiles(_path, fileName + format, SearchOption.AllDirectories);
                if (soundFilePath.Any())
                {
                    FileInfo fileInfo = new FileInfo(soundFilePath[0]);
                    return fileInfo;
                }
            }
            catch (IOException)
            {
                return null;
            }
        }
        return null;

    }
    
    
     /// <summary>
        /// Returns dictionary with all sounds filenames assigned to category, with category as key or category parts as key when filenames list exceed 1024 characters 
        /// </summary>
        /// <param name="categoryPath"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllSoundsFromCategory(string categoryPath)
        {
            Dictionary<string, string> categorySounds = new Dictionary<string, string>();

           List<string> filenames = Directory.GetFiles(categoryPath).Select(f => Path.GetFileName(f)).Where(x => _supportedFormats.Any( format => x.EndsWith(format))).ToList();
           filenames = filenames.OrderBy(filename => filename).ToList();
           StringBuilder filenamesFormated = new StringBuilder();
            string categoryName = Path.GetFileName(categoryPath);

            if (String.IsNullOrWhiteSpace(categoryName))
                categoryName = "Inne";

            int categoryPartCounter = 1;


            foreach (string filename in  filenames)
            {
                //replace normal space with hard space to avoid breaking line in the middle of a phrase
                var formatedFilename = Regex.Replace(filename.Substring(0, filename.Length - 4), " ", "⠀");

                //Checks if filenames list exceed permitted 1024 characters after insertion of new filename
                if (filenamesFormated.Length + formatedFilename.Length > 1021)
                {
                    //Add filenames in parts
                    categorySounds.Add(categoryName + $" cz. {categoryPartCounter}", filenamesFormated.ToString());
                    filenamesFormated.Clear();
                    filenamesFormated.Append("`" + formatedFilename + "` ");
                    categoryPartCounter++;
                }
                else
                    filenamesFormated.Append("`" + formatedFilename + "` ");
            }


            //Checks if there is more than one part, if no then add single part with all filenames
            if (categoryPartCounter == 1)
            {
                categorySounds.Add(categoryName, filenamesFormated.ToString());
            }
            else
            {
                categorySounds.Add(categoryName + $" cz. {categoryPartCounter}", filenamesFormated.ToString());
            }

            return categorySounds;
        }

    
     public bool IsTrackIndetifierLocal(string identifier)
     {
         return identifier.StartsWith(_path);
     }
}

    
    
