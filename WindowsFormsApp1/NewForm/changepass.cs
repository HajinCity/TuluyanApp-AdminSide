using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1.NewForm
{
    public partial class changepass : Form
    {
        private string email; // Current user's email (must be passed in)
        private string apiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg"; // Replace with your real Firebase Web API key

        // Constructor to accept and initialize the email of the current user
        public changepass(string currentUserEmail)
        {
            InitializeComponent();
            email = currentUserEmail;  // Initialize the email from the passed argument
        }

        // Click event handler for the "Change Password" button
        private async void button1_Click(object sender, EventArgs e)
        {
            string currentPassword = textBox1.Text;  // Current password entered by user
            string newPassword = textBox2.Text;      // New password entered by user
            string confirmPassword = textBox3.Text;  // Confirm password entered by user

            // Validate if all fields are filled
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Check if new password and confirm password match
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New password and confirm password do not match.");
                return;
            }

            try
            {
                // Step 1: Reauthenticate the user with the current password and retrieve the idToken
                using (var client = new HttpClient())
                {
                    var loginPayload = new
                    {
                        email = email,  // Ensure the email is correct here
                        password = currentPassword,
                        returnSecureToken = true
                    };

                    var loginJson = JsonConvert.SerializeObject(loginPayload);
                    var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

                    // Firebase API URL for reauthentication
                    var loginResponse = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}", loginContent);
                    var loginResult = await loginResponse.Content.ReadAsStringAsync();

                    if (loginResponse.IsSuccessStatusCode)
                    {
                        // Parse the response and extract the idToken
                        dynamic loginData = JsonConvert.DeserializeObject(loginResult);
                        string idToken = loginData.idToken;

                        // Step 2: Update the password using the idToken
                        var updatePayload = new
                        {
                            idToken = idToken,
                            password = newPassword,
                            returnSecureToken = true
                        };

                        var updateJson = JsonConvert.SerializeObject(updatePayload);
                        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

                        // Firebase API URL for password update
                        var updateResponse = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:update?key={apiKey}", updateContent);
                        var updateResult = await updateResponse.Content.ReadAsStringAsync();

                        // Check if password update was successful
                        if (updateResponse.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Password changed successfully.");
                            this.Close(); // Close the form after success
                        }
                        else
                        {
                            // Handle error from password update
                            dynamic updateError = JsonConvert.DeserializeObject(updateResult);
                            MessageBox.Show("Error updating password: " + updateError.error.message);
                        }
                    }
                    else
                    {
                        // Handle error from reauthentication
                        dynamic loginError = JsonConvert.DeserializeObject(loginResult);
                        string errorMessage = loginError?.error?.message ?? "Unknown error";

                        // Check for common Firebase authentication errors
                        if (errorMessage.Contains("INVALID_EMAIL"))
                        {
                            MessageBox.Show("The email address is invalid. Please check the email format.");
                        }
                        else if (errorMessage.Contains("INVALID_PASSWORD"))
                        {
                            MessageBox.Show("The current password is incorrect. Please try again.");
                        }
                        else
                        {
                            MessageBox.Show("Reauthentication failed: " + errorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch unexpected errors
                MessageBox.Show("Unexpected error: " + ex.Message);
            }
        }

        // Close the form when the picture box is clicked (optional close button)
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
