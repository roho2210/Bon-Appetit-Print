using MySql.Data.MySqlClient;
using ESC_POS_USB_NET.Printer;
using System.Text;
using System.Dynamic;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;

namespace BonPrint
{
    public partial class MainForm : Form
    {
        MySqlConnection cnn;
        Printer printer;
        bool canConnectDB = false;
        bool canPrint = false;
        bool currentMonthTableExist = false;
        bool printedColumnExist = false;
        System.Timers.Timer aTimer;
        DataTable dt = new DataTable();
        int turnsCounter = Properties.Settings.Default.currentTurn;
        string tableName = "";
        string locationText = Properties.Settings.Default.LocationText;
        bool isMonitorRunning = false;

        public MainForm()
        {
            System.Text.EncodingProvider ppp = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(ppp);
            InitializeComponent();

            printerNameTbox.Text = Properties.Settings.Default.PrinterName;
            tableName = "t_lg" + DateTime.Now.ToString("yyyyMM");
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Tarjeta", typeof(string));
            dt.Columns.Add("Hora", typeof(string));
            dt.Columns.Add("Turno", typeof(string));
            dataGridView1.DataSource = dt;

            string printerName = Properties.Settings.Default.PrinterName;
            string connectionString = Properties.Settings.Default.ConnectionString;
            if (connectionString != "")
            {
                connectionString = System.Text.Encoding.UTF8.GetString(Unprotect(System.Convert.FromBase64String(connectionString)));
                this.tryDBconnection(connectionString);
            }
            if (printerName != "")
            {
                this.tryPrinter(printerName);
            }
            if (locationText != "")
            {
                locationTbox.Text = locationText;
                locationTbox.Enabled = false;
                locationSaveBtn.Text = "Eliminar localización";
            }
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (!currentMonthTableExist)
            {
                try
                {
                    cnn.Open();
                    MySqlCommand myCommand = new MySqlCommand("SHOW TABLES LIKE '" + tableName + "';", cnn);
                    MySqlDataReader reader = myCommand.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        cnn.Close();
                        return;
                    }
                    else
                    {
                        currentMonthTableExist = true;
                        cnn.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Hubo un error al revisar si la tabla de registro del mes actual existe.", "Error");
                    cnn.Close();
                    aTimer.Stop();
                    monitorBtn.Text = "Iniciar monitoreo del lector";
                    return;
                }
            }
            if (!printedColumnExist)
            {
                try
                {
                    cnn.Open();
                    MySqlCommand checkColumnCommand = new MySqlCommand("SHOW COLUMNS FROM " + tableName + " LIKE 'PRINTED';", cnn);
                    MySqlDataReader checkColumnReader = checkColumnCommand.ExecuteReader();
                    if (!checkColumnReader.HasRows)
                    {
                        cnn.Close();
                        cnn.Open();
                        MySqlCommand addPrintedColumnCommand = new MySqlCommand("ALTER TABLE " + tableName + " ADD `PRINTED` BOOLEAN NOT NULL DEFAULT \'0\' AFTER `TEMPER`;", cnn);
                        addPrintedColumnCommand.ExecuteNonQuery();
                        cnn.Close();
                        printedColumnExist = true;
                    }
                    else
                    {
                        cnn.Close();
                        printedColumnExist = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Hubo un error al revisar si la columna \"PRINTED\" existe.", "Error");
                    throw;
                }
            }

            try
            {
                dynamic ticketInfo = new ExpandoObject();
                cnn.Open();
                string commandText =
                    "SELECT " + tableName + ".EVTLGUID," + tableName + ".USRID," + tableName + ".PRINTED,t_usr.USRUID,t_usr.NM,t_crd.CRDCSN FROM " + tableName + " " +
                    "LEFT JOIN t_usr ON t_usr.USRID = " + tableName + ".USRID " +
                    "LEFT JOIN t_usrcrd ON t_usrcrd.USRUID = t_usr.USRUID " +
                    "LEFT JOIN t_crd ON t_crd.CRDUID = t_usrcrd.CRDUID " +
                    "WHERE EVT = 4102 ORDER BY EVTLGUID DESC LIMIT 1;";
                MySqlCommand lastCardSuccesfulReadCommand = new MySqlCommand(commandText, cnn);
                MySqlDataReader lastCardSuccesfulReadReader = lastCardSuccesfulReadCommand.ExecuteReader();
                if (lastCardSuccesfulReadReader.Read())
                {
                    if ((bool)lastCardSuccesfulReadReader["PRINTED"])
                    {
                        lastCardSuccesfulReadCommand.Dispose();
                        cnn.Close();
                        return;
                    }
                    System.Diagnostics.Debug.WriteLine(lastCardSuccesfulReadReader["USRID"]);
                    ticketInfo.eventLogId = lastCardSuccesfulReadReader["EVTLGUID"];
                    ticketInfo.UserId = lastCardSuccesfulReadReader["USRID"];
                    System.Diagnostics.Debug.WriteLine(lastCardSuccesfulReadReader["NM"]);
                    ticketInfo.NM = lastCardSuccesfulReadReader["NM"];
                    ticketInfo.CRDCSN = lastCardSuccesfulReadReader["CRDCSN"];
                    cnn.Close();
                }
                else
                {
                    lastCardSuccesfulReadCommand.Dispose();
                    cnn.Close();
                    return;
                }

                printer.Clear();
                printer.DoubleWidth2();
                printer.AlignCenter();
                printer.BoldMode("BONAPPETIT INTERCERAMIC");
                printer.Append("  ");
                printer.Append("  ");
                printer.Append(ticketInfo.NM);
                printer.Append(ticketInfo.CRDCSN);
                printer.Append(DateTime.Now.ToString("t"));
                printer.Append("Turno " + turnsCounter);
                printer.Append(locationText);
                printer.Append("  ");
                printer.Append("  ");
                printer.FullPaperCut();
                printer.PrintDocument();

                cnn.Open();
                MySqlCommand updatePrintedColumn = new MySqlCommand("UPDATE " + tableName + " SET printed = 1 WHERE EVTLGUID = " + ticketInfo.eventLogId + ";", cnn);
                updatePrintedColumn.ExecuteNonQuery();
                cnn.Close();
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        dt.Rows.Add(new object[] { ticketInfo.NM, ticketInfo.CRDCSN, DateTime.Now.ToString("t"), turnsCounter });
                    }));
                }
                else
                {
                    dt.Rows.Add(new object[] { ticketInfo.NM, ticketInfo.CRDCSN, DateTime.Now.ToString("t"), turnsCounter });
                }
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        turnsLbl.Text = "Turno " + turnsCounter;
                    }));
                }
                else
                {
                    turnsLbl.Text = "Turno " + turnsCounter;
                }
                turnsCounter++;
                Properties.Settings.Default.currentTurn = turnsCounter;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                this.LogWriter(ex.ToString());
                aTimer.Stop();
                MessageBox.Show("Hubo un error al realizar la impresión del ticket.", "Error");
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        this.isMonitorRunning = false;
                        monitorBtn.Text = "Iniciar monitoreo del lector";
                    }));
                }
                else
                {
                    this.isMonitorRunning = false;
                    monitorBtn.Text = "Iniciar monitoreo del lector";
                }
                return;
            }
        }

        private void MonitorBtn_Click(object sender, EventArgs e)
        {
            if (!this.canConnectDB)
            {
                MessageBox.Show("Asegurate de que puedes conectarte a la base de datos correctamente.", "Error");
                return;
            }
            if (!this.canPrint)
            {
                MessageBox.Show("Asegurate de que puedes imprimir correctamente.", "Error");
                return;
            }
            if (this.locationText == "")
            {
                MessageBox.Show("La localización no se ha configurado.", "Error");
                return;
            }

            if (aTimer != null)
            {
                if (aTimer.Enabled)
                {
                    aTimer.Stop();
                    monitorBtn.Text = "Iniciar monitoreo del lector";
                    this.isMonitorRunning = false;
                    return;
                }
                else
                {
                    aTimer.Start();
                    monitorBtn.Text = "Detener monitoreo del lector";
                    this.isMonitorRunning = true;
                    return;
                }
            }
            else
            {
                aTimer = new System.Timers.Timer();
                aTimer.Interval = 1000;
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Start();
                this.isMonitorRunning = true;
                monitorBtn.Text = "Detener monitoreo del lector";
            }
        }

        private void Connect(object sender, EventArgs e)
        {
            if (this.isMonitorRunning)
            {
                MessageBox.Show("Es necesario detener el monitor para realizar cambios.");
                return;
            }
            if (this.canConnectDB)
            {
                connectBtn.Text = "Probar y guardar conexión";
                this.canConnectDB = false;
                cnn.ConnectionString = "";
                userTbox.Enabled = true;
                passwordTbox.Enabled = true;
                Properties.Settings.Default.ConnectionString = "";
                Properties.Settings.Default.Save();
                return;
            }
            if (userTbox.Text == "")
            {
                MessageBox.Show("Los campos usuario y contraseña son necesarios para conectar con la base de datos.");
                return;
            }
            string connectionString = "server=localhost;database=biostar2_ac;uid=" + userTbox.Text + ";pwd=\"" + passwordTbox.Text + "\";Port=3312";
            this.tryDBconnection(connectionString);
        }

        private void PrintTestBtn_Click(object sender, EventArgs e)
        {
            if (this.isMonitorRunning)
            {
                MessageBox.Show("Es necesario detener el monitor para realizar cambios.");
                return;
            }
            if (printerNameTbox.Text == "")
            {
                MessageBox.Show("El nombre de la impresora no puede estar vacío.", "Error");
                return;
            }
            if (this.canPrint)
            {
                printTestBtn.Text = "Probar y guardar impresora";
                printerNameTbox.Enabled = true;
                this.canPrint = false;
            }
            else
            {
                this.tryPrinter(printerNameTbox.Text);
            }
        }

        private void LocationSaveBtn_Click(object sender, EventArgs e)
        {
            if (this.isMonitorRunning)
            {
                MessageBox.Show("Es necesario detener el monitor para realizar cambios.");
                return;
            }
            if (locationText != "")
            {
                locationText = "";
                locationTbox.Enabled = true;
                locationTbox.Text = locationText;
                locationSaveBtn.Text = "Guardar localización";
                Properties.Settings.Default.LocationText = locationText;
                Properties.Settings.Default.Save();
            }
            else
            {
                locationText = locationTbox.Text;
                if (locationText == "")
                {
                    MessageBox.Show("La campo localización no puede ser vacío", "Error");
                    return;
                }
                locationTbox.Enabled = false;
                locationSaveBtn.Text = "Eliminar localización";
                Properties.Settings.Default.LocationText = locationText;
                Properties.Settings.Default.Save();
            }
        }

        private bool tryDBconnection(string connectionString)
        {

            try
            {
                cnn = new MySqlConnection(connectionString);
                cnn.Open();
                cnn.Close();
                this.canConnectDB = true;
                userTbox.Enabled = false;
                userTbox.Text = "";
                passwordTbox.Enabled = false;
                passwordTbox.Text = "";
                connectBtn.Text = "Eliminar conexión";
                Properties.Settings.Default.ConnectionString = System.Convert.ToBase64String(Protect(Encoding.UTF8.GetBytes(connectionString)));
                Properties.Settings.Default.Save();
                //MessageBox.Show("Conectado correctamente", "Éxito");
                return true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                string dbConnectionError = "Hubo un error con la base de datos: ";
                switch (ex.Number)
                {
                    case 0:
                        dbConnectionError += "No se pudo conectar con la base de datos.";
                        break;
                    case 1042:
                        dbConnectionError += "No se pudo conectar con el host.";
                        break;
                    case 1045:
                        dbConnectionError += "Usuario o contraseña inválidos.";
                        break;
                    default:
                        dbConnectionError += ex.Number;
                        break;
                }
                MessageBox.Show(dbConnectionError);
                this.canConnectDB = false;
                return false;
            }
        }
        private bool tryPrinter(string printerName)
        {
            try
            {
                printer = new Printer(printerName);
                printer.Append("Conectado");
                printer.FullPaperCut();
                printer.PrintDocument();
                canPrint = true;
                printTestBtn.Text = "Eliminar impresora";
                printerNameTbox.Enabled = false;
                Properties.Settings.Default.PrinterName = printerName;
                Properties.Settings.Default.Save();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                canPrint = false;
                MessageBox.Show("Hubo un error al realizar la impresión, revisa que el nombre de la impresora sea el correcto.", "Error");
                return false;
            }
        }

        public void LogWriter(string logMessage)
        {
            LogWrite(logMessage);
        }

        public void LogWrite(string logMessage)
        {
            string m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Debug.WriteLine(m_exePath);
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
            }
        }

        private void resetCounterBtn_Click(object sender, EventArgs e)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    turnsLbl.Text = "Turno -";
                    this.turnsCounter = 1;
                    Properties.Settings.Default.currentTurn = this.turnsCounter;
                    Properties.Settings.Default.Save();
                    dt.Clear();
                }));
            }
            else
            {
                turnsLbl.Text = "Turno -";
                this.turnsCounter = 1;
                Properties.Settings.Default.currentTurn = this.turnsCounter;
                Properties.Settings.Default.Save();
                dt.Clear();
            }
        }
        public static byte[] Protect(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static byte[] Unprotect(byte[] data)
        {
            try
            {
                return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
