using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clinica.Formularios
{
    public partial class Menu : Form
    {
        Clases.ConexionSQL x = new Clases.ConexionSQL();
        SqlConnection con = new SqlConnection();

        //la clave y el iv los puse figos para no batallar en la base
        //de datos
        private static readonly byte[] key = Encoding.UTF8.GetBytes("ClaveScret_Nigga");
        private static readonly byte[] iv = Encoding.UTF8.GetBytes("IV_Nigga");


        private const int EM_SETCUEBANNER = 0x1501;

        // Constantes para arrastrar la ventana
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public Menu()
        {
            InitializeComponent();
            CargarSexo();
            CargarPacientes();

            txtNss.MaxLength = 0;
            txtTelefono.MaxLength = 0;
            txtCurp.MaxLength = 0;
            txtNota.MaxLength = 0;
        }

        private string Encriptar(string plainText)
        {
            using (RC2 rc2 = RC2.Create())
            {
                rc2.Key = key;
                rc2.IV = iv;

                ICryptoTransform encryptor = rc2.CreateEncryptor(rc2.Key, rc2.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    //esta cosa convbierte el texto cifrado a base 64
                    //para que gusrden la base de datos sin pedos
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string Desencriptar(string cipherBase64)
        {
            // esto convbierte lo de la base de dtaos a bytes
            byte[] cipherText = Convert.FromBase64String(cipherBase64);

            using (RC2 rc2 = RC2.Create())
            {
                rc2.Key = key;
                rc2.IV = iv;

                ICryptoTransform decryptor = rc2.CreateDecryptor(rc2.Key, rc2.IV);

                using (MemoryStream ms = new MemoryStream(cipherText))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        //che sexo nada de terians o jotos
        private void CargarSexo()
        {
            cbSexo.Items.Clear();
            cbSexo.Items.Add("Masculino");
            cbSexo.Items.Add("Femenino");
            cbSexo.Items.Add("Otro");
            cbSexo.SelectedIndex = 0;
        }


        private void ArrastrarFormulario(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnCliente_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            //Application.Exit();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            //WindowState = FormWindowState.Minimized;
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            // Asociar el evento MouseDown al formulario y al panel
            this.MouseDown += ArrastrarFormulario;
            panell1.MouseDown += ArrastrarFormulario;
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            //encripta antes de que se guarde en base de datos
            //en este caso CURP

            //borracha nomas duplica esta linea de codigo por cada campo que 
            //tiene la llave 
            string curpCifrada = string.IsNullOrEmpty(txtCurp.Text) ? "" : Encriptar(txtCurp.Text);


            //Aqui agregale los campos que flatal ya no se me ocurren insultos
            string sql = @"insert into Pacientes (Nombre, ApellidoMa, ApellidoPa, FechaNacimiento, Sexo, CURP) values (@nombre, @apellidoMa, @apellidoPa, @fechaNac, @sexo, @curp)";

            con = new SqlConnection(x.Conexion);
            con.Open();


            //Igual aqui agregale los campos que flatan
            SqlCommand p = new SqlCommand(sql, con);
            p.Parameters.AddWithValue("@nombre", txtNombre.Text);
            p.Parameters.AddWithValue("@apellidoMa", txtApellidoMa.Text);
            p.Parameters.AddWithValue("@apellidoPa", txtApellidoPa.Text);
            p.Parameters.AddWithValue("@fechaNac", dtFellaNa.Value);
            p.Parameters.AddWithValue("@sexo", cbSexo.SelectedItem.ToString());
            p.Parameters.AddWithValue("@curp", curpCifrada);

            p.ExecuteNonQuery();
            con.Close();

            MessageBox.Show("Paciente guardado exitosamente.");
            Limpiar();
            CargarPacientes();

        }

        private void btnDecifrar_Click(object sender, EventArgs e)
        {
            if(dtgPaciente.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un Paciente de la Tabla.");
                return;
            }
            int id = Convert.ToInt32(dtgPaciente.SelectedRows[0].Cells["Id"].Value);

            //Aqui nomas ponle todos los campos que se cifraron yo le puse la curp para ver que
            //que me traiga la cosa cifrada
            string sql = "select CURP from Pacientes where id = @id";

            con = new SqlConnection(x.Conexion);
            con.Open();

            SqlCommand p = new SqlCommand(sql, con);
            p.Parameters.AddWithValue("@id", id);


            //Desencripchon todo pue y agrega un strign por cada campo encriptadpo
            SqlDataReader re = p.ExecuteReader();
            if (re.Read())
            {
                string curpOriginal = Desencriptar(re["CURP"].ToString());

                MessageBox.Show(
                    $"CURP: {curpOriginal}\n" //+
                    //$"NSS: {curpOriginal}\n"

                    // asi mero llenalo con todos los demas campos
                );
            }
            re.Close();
            con.Close();
           
        }

        private void CargarPacientes()
        {
            string sql = @"select id, CONCAT(Nombre, ' ' ,ApellidoMa, ' ' ,ApellidoPa) as Nombre, FechaNacimiento from Pacientes";

            con = new SqlConnection(x.Conexion);
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dtgPaciente.DataSource = dt;
            dtgPaciente.Columns["id"].Visible = false;

            con.Close();

        }
        private void BuscarPacientes()
        {
            string sql = @"select id, CONCAT(Nombre, ' ' ,ApellidoMa, ' ' ,ApellidoPa) as Nombre, FechaNacimiento FROM Pacientes WHERE Nombre LIKE @filtro OR ApellidoMa LIKE @filtro OR ApellidoPa LIKE @filtro ";

            con = new SqlConnection(x.Conexion);
            con.Open();

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@filtro", "%" + txtBuscar.Text.Trim() + "%");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dtgPaciente.DataSource = dt;
            dtgPaciente.Columns["id"].Visible = false;

            con.Close();
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            BuscarPacientes();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                BuscarPacientes();
        }

        private void Limpiar()
        {
            txtNombre.Clear();
            txtApellidoMa.Clear();
            txtApellidoPa.Clear();
            txtCurp.Clear();
            txtNss.Clear();
            txtTelefono.Clear();
            txtDiagnosticoPri.Clear();
            txtNota.Clear();
            cbSexo.SelectedIndex = 0;
            dtFellaNa.Value = DateTime.Today;
            txtBuscar.Clear();
            CargarPacientes();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void txtNombre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void txtApellidoMa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void txtApellidoPa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void txtTelefono_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void dtgPaciente_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = dtgPaciente.CurrentRow.Index;
            dtgPaciente.Rows[i].Selected = true;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (dtgPaciente.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un paciente de la tabla.");
                return;
            }

            int id = Convert.ToInt32(dtgPaciente.SelectedRows[0].Cells["Id"].Value);

            string sql = "select * from Pacientes where id = @id";

            con = new SqlConnection(x.Conexion);
            con.Open();

            SqlCommand p = new SqlCommand(sql, con);
            p.Parameters.AddWithValue("@id", id);

            SqlDataReader re = p.ExecuteReader();

            if (re.Read())
            {
                txtNombre.Text = re["Nombre"].ToString();
                txtApellidoMa.Text = re["ApellidoMa"].ToString();
                txtApellidoPa.Text = re["ApellidoPa"].ToString();
                cbSexo.Text = re["Sexo"].ToString();
                dtFellaNa.Value = Convert.ToDateTime(re["FechaNacimiento"]);

                // los campos cifrados se miran tal cual
                txtCurp.Text = re["CURP"].ToString();
            }
        }
    }
}
