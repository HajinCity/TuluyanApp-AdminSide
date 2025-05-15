using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();
            string password = textBox2.Text;
            string confirmPassword = textBox3.Text;
            string name = textBox4.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var payload = new
                    {
                        email = email,
                        password = password,
                        returnSecureToken = true
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var apiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg";
                    var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        string uid = result.localId;

                        try
                        {
                            DocumentReference docRef = FirebaseInitialization.Database.Collection("users").Document(uid);
                            Dictionary<string, object> userData = new Dictionary<string, object>
                            {
                                { "email", email },
                                { "name", name },
                                { "role", "admin" },
                                { "createdAt", Timestamp.GetCurrentTimestamp() }
                            };

                            await docRef.SetAsync(userData);
                            MessageBox.Show("Account created and saved to Firestore!");

                            // Clear all textboxes after success
                            textBox1.Clear();
                            textBox2.Clear();
                            textBox3.Clear();
                            textBox4.Clear();
                        }
                        catch (Exception firestoreEx)
                        {
                            MessageBox.Show("Account created, but Firestore error: " + firestoreEx.Message);
                        }
                    }
                    else
                    {
                        dynamic error = JsonConvert.DeserializeObject(responseString);
                        MessageBox.Show($"Error: {error.error.message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

      
    }
}
