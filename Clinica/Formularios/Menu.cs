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

            txtNss.MaxLength = 11; //El NSS, Telefono y Curp tienen límite de digítos, así que le cambié aquí (puesto que desde las propiedades del diseñador no lo aplicaba, ya que lo estabas sobrescribiendo con 0 al inicializarlo)
            txtTelefono.MaxLength = 10; 
            txtCurp.MaxLength = 18;
            txtCurp.CharacterCasing = CharacterCasing.Upper; //Le agregué esto para que siempre se ponga en mayuscula, aunque el teclado este desactivado, puesto que por estética la CURP es así
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
        //XD
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
            txtNombre.Focus(); //Le agregué el focus para que lo inicie en el primer campo a llenar, por estética
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
            //Validación para que no guarde un "registro" vacío :v
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellidoPa.Text) || string.IsNullOrWhiteSpace(txtApellidoMa.Text) || string.IsNullOrWhiteSpace(txtCurp.Text) || string.IsNullOrWhiteSpace(txtNss.Text) || string.IsNullOrWhiteSpace(txtDiagnosticoPri.Text) || string.IsNullOrWhiteSpace(txtTelefono.Text) || string.IsNullOrWhiteSpace(txtNota.Text))
            {
                MessageBox.Show("No puede haber campos vacíos. Por favor ingrese todos los datos requeridos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Validaciones para que no se guarden datos con formato incorrecto
            if (txtCurp.Text.Length < 18)
            {
                MessageBox.Show("La CURP debe tener 18 caracteres, para ser correcta.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCurp.Focus();
                return;
            }
            if (txtNss.Text.Length < 11)
            {
                MessageBox.Show("El NSS debe ser de 11 caracteres, para ser correcto.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNss.Focus();
                return;
            }
            if (txtTelefono.Text.Length < 10)
            {
                MessageBox.Show("El teléfono debe tener 10 dígitos, para ser correcto.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefono.Focus();
                return;
            }

            //encripta antes de que se guarde en base de datos
            //en este caso CURP
            //borracha nomas duplica esta linea de codigo por cada campo que (No soy Borracha :v)
            //tiene la llave
            //ENTENDIDO :)
            string curpCifrada = string.IsNullOrEmpty(txtCurp.Text) ? "" : Encriptar(txtCurp.Text);
            string nss = string.IsNullOrEmpty(txtNss.Text) ? "" : Encriptar(txtNss.Text);
            string diagnostico = string.IsNullOrEmpty(txtDiagnosticoPri.Text) ? "" : Encriptar(txtDiagnosticoPri.Text);
            string telefono = string.IsNullOrEmpty(txtTelefono.Text) ? "" : Encriptar(txtTelefono.Text);
            string notaMedica = string.IsNullOrEmpty(txtNota.Text) ? "" : Encriptar(txtNota.Text);


            //Aqui agregale los campos que flatal ya no se me ocurren insultos (xD)
            string sql = @"insert into Pacientes (Nombre, ApellidoMa, ApellidoPa, FechaNacimiento, Sexo, CURP, NSS, Diagnostico, Telefono, Notas) values (@nombre, @apellidoMa, @apellidoPa, @fechaNac, @sexo, @curp, @nss, @diagnostico, @telefono, @notas)";

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
            p.Parameters.AddWithValue("@nss", nss);
            p.Parameters.AddWithValue("@diagnostico", diagnostico);
            p.Parameters.AddWithValue("@telefono", telefono);
            p.Parameters.AddWithValue("@notas", notaMedica);

            p.ExecuteNonQuery();
            con.Close();

            MessageBox.Show("Paciente guardado exitosamente.", "MENSAJE",MessageBoxButtons.OK, MessageBoxIcon.Information); //Le agregué el ícono al mensaje, por estética
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
            string sql = "select CURP, NSS, Diagnostico, Telefono, Notas from Pacientes where id = @id";

            con = new SqlConnection(x.Conexion);
            con.Open();

            SqlCommand p = new SqlCommand(sql, con);
            p.Parameters.AddWithValue("@id", id);


            //Desencripchon todo pue y agrega un strign por cada campo encriptadpo
            SqlDataReader re = p.ExecuteReader();
            if (re.Read())
            {
                string curpOriginal = Desencriptar(re["CURP"].ToString());
                string nssOriginal = Desencriptar(re["NSS"].ToString());
                string diagnosticoOriginal = Desencriptar(re["Diagnostico"].ToString());
                string telefonoOriginal = Desencriptar(re["Telefono"].ToString());
                string notaMOriginal = Desencriptar(re["Notas"].ToString());

                MessageBox.Show(
                    $"CURP: {curpOriginal}\n" + $"NSS: {nssOriginal}\n" + $"Diagnostico: {diagnosticoOriginal}\n" + $"Telefono: {telefonoOriginal}\n" + $"Notas: {notaMOriginal}","RESULTADOS", MessageBoxButtons.OK, MessageBoxIcon.Information //También le agregué el ícono al mensaje, por estética

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
            txtNombre.Focus(); //Le agregué el focus por estética
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
                txtNss.Text = re["NSS"].ToString();
                txtDiagnosticoPri.Text = re["Diagnostico"].ToString();
                txtTelefono.Text = re["Telefono"].ToString();
                txtNota.Text = re["Notas"].ToString();
            }
        }

        //Le agregué este evento para que solo permita números en el NSS
        private void txtNss_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
    }
}
