using System;
using System.IO;
using System.Windows.Forms;
using Google.Cloud.Firestore;

namespace WindowsFormsApp1
{
    public static class FirebaseInitialization
    {
        private static FirestoreDb _firestoreDb;

        static FirebaseInitialization()
        {
            try
            {
                string credentialsPath = @"C:\Users\WINDOWS 10\source\repos\Hestia\WindowsFormsApp1\WindowsFormsApp1\FirebaseJSONFile\tuluyan-user-login-firebase-adminsdk-e385i-fba6babab6.json";

                if (!File.Exists(credentialsPath))
                {
                    throw new FileNotFoundException("Firebase credentials file not found at path: " + credentialsPath);
                }

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

                string projectId = "tuluyan-user-login";
                _firestoreDb = FirestoreDb.Create(projectId);
                Console.WriteLine("Firebase Firestore initialized successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize Firebase: " + ex.Message);
            }
        }

        public static FirestoreDb Database
        {
            get
            {
                if (_firestoreDb == null)
                {
                    throw new InvalidOperationException("Firestore is not initialized properly.");
                }
                return _firestoreDb;
            }
        }
    }
}
