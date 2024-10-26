namespace RingInterceptorMaui
{
    public class FilePaths
    {
        private static readonly string RefreshTokenFileName = "RefreshToken.txt";
        public static readonly string RootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string RefreshTokenPath => Path.Combine(RootDirectory, RefreshTokenFileName);
    }
}
