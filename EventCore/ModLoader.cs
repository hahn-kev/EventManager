namespace EventCore
{
    public class ModLoader
    {
        public ModRoot Load(string folderPath)
        {

            return new ModRoot(folderPath);
        }
    }
}
