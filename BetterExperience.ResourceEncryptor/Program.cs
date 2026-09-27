namespace BetterExperience.ResourceEncryptor
{
    internal static class Program
    {
        private const string Usage = "Usage: BetterExperience.ResourceEncryptor encrypt --input <resource-root> --output <new-directory>";

        private static int Main(string[] args) => Run(args, Console.Out, Console.Error);

        internal static int Run(string[] args, TextWriter output, TextWriter error)
        {
            if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
            {
                output.WriteLine(Usage);
                return 0;
            }
            if (args.Length != 5 || args[0] != "encrypt")
            {
                error.WriteLine(Usage);
                return 2;
            }
            string input = null, destination = null;
            for (int i = 1; i < args.Length; i += 2)
            {
                if (args[i] == "--input" && input == null) input = args[i + 1];
                else if (args[i] == "--output" && destination == null) destination = args[i + 1];
                else { error.WriteLine(Usage); return 2; }
            }
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(destination))
            {
                error.WriteLine(Usage);
                return 2;
            }
            try
            {
                int count = PackEncryptor.Encrypt(input, destination, path => output.WriteLine("Encrypted: " + path));
                output.WriteLine("Completed: " + count + " files -> " + Path.GetFullPath(destination));
                return 0;
            }
            catch (Exception ex)
            {
                error.WriteLine("Encryption failed: " + ex.Message);
                return 1;
            }
        }
    }
}
