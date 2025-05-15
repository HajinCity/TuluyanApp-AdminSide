using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.NewForm;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        private string _uid;
        private Form activeForm;

        public Form3(string uid)
        {
            InitializeComponent();
            _uid = uid;

            // Fire and forget async task to load user data
            LoadUserName();
            home_btn_Click(null, EventArgs.Empty);
        }

        private async void LoadUserName()
        {
            try
            {
                DocumentSnapshot userDoc = await FirebaseInitialization.Database
                    .Collection("users")
                    .Document(_uid)
                    .GetSnapshotAsync();

                if (userDoc.Exists && userDoc.TryGetValue("name", out string name))
                {
                    label2.Text = name; // Set the name in label2
                }
                else
                {
                    label2.Text = "Name not found";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load user name: " + ex.Message);
            }
        }

        private void openingForm(Form childForm)
        {
            if (activeForm != null)

                activeForm.Close();

            activeForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel3.Controls.Add(childForm);
            panel3.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void home_btn_Click(object sender, EventArgs e)
        {
            openingForm(new HomeDashboard());
        }

        private void management_btn_Click(object sender, EventArgs e)
        {
            openingForm(new Form2());
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            // Optional: Confirm logout
            var confirm = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                this.Hide(); // Hide the current Form3
                Form1 loginForm = new Form1(); // Create a new instance of Form1
                loginForm.Show(); // Show Form1 (login screen)
            }
        }

    }
}
