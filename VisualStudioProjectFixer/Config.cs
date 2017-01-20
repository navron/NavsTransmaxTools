namespace VisualStudioProjectFixer
{
    public static class Config
    {
        public static string[] GetSourceSearchPatterns
        {
            get { return new string[] {"*.csproj", "*.config", "*.Config"}; }
        }
    }
}