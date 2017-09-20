using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace EncryptPassword
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (rdLogin.Checked)
            {
                if (isValidPassword(username, pass))
                {
                    lblResultado.Text = $"Bienvenido {username} , su password ingresado es correcto";
                }
                else
                {
                    lblResultado.Text = $"Usuario y/o password son incorrectos";
                }
            }
            else
            {
                if (saveUser(username, pass))                
                    lblResultado.Text = $"Se registro correctamente al usuario {username} con su password {pass}";
                                
            }

        }

        private bool isValidPassword(string username, string password)
        {
            UserBE user = getUserFromDB(username);
            bool isValid = false;

            if (!string.IsNullOrEmpty(user.user)) {                 
            byte[] hashedPassword = Cryptographic.HashPasswordWithSalt(Encoding.UTF8.GetBytes(password), user.salt);
             
            if (hashedPassword.SequenceEqual(user.pass))
                isValid = true;
            }

            return isValid;
            
        }

        private UserBE getUserFromDB(string username)
        {
            UserBE user = new UserBE();

            var connectionString = ConfigurationManager.ConnectionStrings["DemoCredentials"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string saltSaved = "select username, salt, pass from users where username = @username";

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = saltSaved;
                    command.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;

                    try
                    {
                        connection.Open();
                        using (SqlDataReader oReader = command.ExecuteReader())
                        {
                            if (oReader.Read())
                            {
                                user.user = oReader["username"].ToString();
                                user.salt = (byte[])oReader["salt"];
                                user.pass = (byte[])oReader["pass"];
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        lblResultado.Text = ex.Message;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return user;
        }

        private void rdLogin_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '*';
            btnIngresar.Text = "Ingresar";
        }

        private void rdRegistrar_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '\0' ;
            btnIngresar.Text = "Registrar";
        }

        private bool saveUser(string user, string password)
        {
            bool isSaved = false;
            byte[] salt = Cryptographic.GenerateSalt();
            var hashedPassword = Cryptographic.HashPasswordWithSalt(Encoding.UTF8.GetBytes(password), salt);

            var connectionString = ConfigurationManager.ConnectionStrings["DemoCredentials"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string saveUser = "INSERT into users (username,salt,pass) VALUES (@username,@salt,@pass)";

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = saveUser;
                    command.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = user;
                    command.Parameters.Add("@salt", SqlDbType.VarBinary).Value = salt;
                    command.Parameters.Add("@pass", SqlDbType.VarBinary).Value = hashedPassword;

                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                        if (recordsAffected > 0)
                            isSaved = true;
                        
                    }
                    catch (SqlException ex)
                    {
                        lblResultado.Text = ex.Message;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return isSaved;
        }

    }
}
