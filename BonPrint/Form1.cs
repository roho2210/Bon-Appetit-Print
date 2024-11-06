using MySql.Data.MySqlClient;
using ESC_POS_USB_NET.Printer;
using System.Text;
using System.Dynamic;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using MySqlX.XDevAPI.Relational;
using System.Globalization;

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
        int turnsCounter = Properties.Settings.Default.CurrentTurn;
        string tableName = "";
        string locationText = Properties.Settings.Default.LocationText;
        bool isMonitorRunning = false;
        StringWriter writer = new StringWriter();
        DateTime lastRestartDate = Properties.Settings.Default.LastRestartDate;

        public MainForm()
        {
            System.Text.EncodingProvider ppp = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(ppp);
            InitializeComponent();
            printerNameTbox.Text = Properties.Settings.Default.PrinterName;
            tableName = "t_lg" + DateTime.Now.ToString("yyyyMM");
            dt.TableName = "Historial de tickets";
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Empleado", typeof(string));
            dt.Columns.Add("Hora", typeof(string));
            dt.Columns.Add("Turno", typeof(string));
            dt.Columns.Add("Consumo", typeof(string));
            dataGridView1.DataSource = dt;

            string printerName = Properties.Settings.Default.PrinterName;
            string connectionString = Properties.Settings.Default.ConnectionString;
            string tableDataXML = Properties.Settings.Default.TableDataXML;
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
                locationSaveBtn.Text = "Eliminar localizaci�n";
            }
            if(tableDataXML != "")
            {
                StringReader reader = new StringReader(tableDataXML);
                dt.ReadXml(reader);
            }
            if(lastRestartDate != DateTime.MinValue) {
                lastResetDateLbl.Text = "Desde " +lastRestartDate.ToString("g", new CultureInfo("es-MX"));
            }
            if(turnsCounter > 1)
            {
                turnsLbl.Text = "Turno " + (turnsCounter -1);
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
                    cnn.Close();
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    this.LogWriter(ex.ToString());
                    aTimer.Stop();
                    MessageBox.Show("Hubo un error al revisar si la tabla de registro del mes actual existe.", "Error");
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.isMonitorRunning = false;
                            monitorBtn.Text = "Iniciar monitoreo del lector";
                            monitorBtn.BackColor = Color.PaleGreen;
                        }));
                    }
                    else
                    {
                        this.isMonitorRunning = false;
                        monitorBtn.Text = "Iniciar monitoreo del lector";
                        monitorBtn.BackColor = Color.PaleGreen;
                    }
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
                    cnn.Close();
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    this.LogWriter(ex.ToString());
                    aTimer.Stop();
                    MessageBox.Show("Hubo un error al revisar si la columna \"PRINTED\" existe.", "Error");
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.isMonitorRunning = false;
                            monitorBtn.Text = "Iniciar monitoreo del lector";
                            monitorBtn.BackColor = Color.PaleGreen;
                        }));
                    }
                    else
                    {
                        this.isMonitorRunning = false;
                        monitorBtn.Text = "Iniciar monitoreo del lector";
                        monitorBtn.BackColor = Color.PaleGreen;
                    }
                    return;
                }
            }

            try
            {
                if ((lastRestartDate < DateTime.Today.AddHours(10).AddMinutes(00)) && (DateTime.Now > DateTime.Today.AddHours(10).AddMinutes(00)))
                {
                    resetHistory();
                    lastRestartDate = DateTime.Now;
                    if (InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            lastResetDateLbl.Text = "Desde " + lastRestartDate.ToString("g", new CultureInfo("es-MX"));
                        }));
                    }
                    else
                    {
                        lastResetDateLbl.Text = "Desde " + lastRestartDate.ToString("g", new CultureInfo("es-MX"));
                    }
                    Properties.Settings.Default.LastRestartDate = lastRestartDate;
                    Properties.Settings.Default.Save();
                }
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
                    ticketInfo.eventLogId = lastCardSuccesfulReadReader["EVTLGUID"];
                    ticketInfo.UserId = lastCardSuccesfulReadReader["USRID"];
                    ticketInfo.NM = lastCardSuccesfulReadReader["NM"];
                    ticketInfo.USRID = lastCardSuccesfulReadReader["USRID"];
                    cnn.Close();
                }
                else
                {
                    lastCardSuccesfulReadCommand.Dispose();
                    cnn.Close();
                    return;
                }
                int consuptionNumber = dt.Select("Empleado = '"+ ticketInfo.USRID + "'").Length + 1;
                
                printer.Clear();
                printer.DoubleWidth2();
                printer.AlignCenter();
                printer.BoldMode("BONAPPETIT INTERCERAMIC");
                printer.Append("  ");
                printer.Append("  ");
                printer.Append(ticketInfo.NM);
                printer.Append(ticketInfo.USRID);
                printer.Append(DateTime.Now.ToString("g", new CultureInfo("es-MX")));
                printer.Append("Turno " + turnsCounter);
                printer.Append(locationText);
                printer.Append("Consumo " + consuptionNumber);
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
                        dt.Rows.Add(new object[] { ticketInfo.NM, ticketInfo.USRID, DateTime.Now.ToString("t"), turnsCounter, consuptionNumber });
                        turnsLbl.Text = "Turno " + turnsCounter;
                    }));
                }
                else
                {
                    dt.Rows.Add(new object[] { ticketInfo.NM, ticketInfo.USRID, DateTime.Now.ToString("t"), turnsCounter, consuptionNumber });
                    turnsLbl.Text = "Turno " + turnsCounter;
                }

                writer.GetStringBuilder().Clear();
                dt.WriteXml(writer);
                Properties.Settings.Default.TableDataXML = "<TableData>" + writer.ToString() + "</TableData>";

                turnsCounter++;
                Properties.Settings.Default.CurrentTurn = turnsCounter;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                this.LogWriter(ex.ToString());
                aTimer.Stop();
                MessageBox.Show("Hubo un error al realizar la impresi�n del ticket.", "Error");
                if (InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        this.isMonitorRunning = false;
                        monitorBtn.Text = "Iniciar monitoreo del lector";
                        monitorBtn.BackColor = Color.PaleGreen;
                    }));
                }
                else
                {
                    this.isMonitorRunning = false;
                    monitorBtn.Text = "Iniciar monitoreo del lector";
                    monitorBtn.BackColor = Color.PaleGreen;
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
                MessageBox.Show("La localizaci�n no se ha configurado.", "Error");
                return;
            }

            if (aTimer != null)
            {
                if (aTimer.Enabled)
                {
                    aTimer.Stop();
                    monitorBtn.Text = "Iniciar monitoreo del lector";
                    this.isMonitorRunning = false;
                    monitorBtn.BackColor = Color.PaleGreen;
                    return;
                }
                else
                {
                    aTimer.Start();
                    monitorBtn.Text = "Detener monitoreo del lector";
                    this.isMonitorRunning = true;
                    monitorBtn.BackColor = Color.LightCoral;
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
                monitorBtn.BackColor = Color.LightCoral;
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
                connectBtn.Text = "Probar y guardar conexi�n";
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
                MessageBox.Show("Los campos usuario y contrase�a son necesarios para conectar con la base de datos.");
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
                MessageBox.Show("El nombre de la impresora no puede estar vac�o.", "Error");
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
                locationSaveBtn.Text = "Guardar localizaci�n";
                Properties.Settings.Default.LocationText = locationText;
                Properties.Settings.Default.Save();
            }
            else
            {
                locationText = locationTbox.Text;
                if (locationText == "")
                {
                    MessageBox.Show("La campo localizaci�n no puede ser vac�o", "Error");
                    return;
                }
                locationTbox.Enabled = false;
                locationSaveBtn.Text = "Eliminar localizaci�n";
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
                connectBtn.Text = "Eliminar conexi�n";
                Properties.Settings.Default.ConnectionString = System.Convert.ToBase64String(Protect(Encoding.UTF8.GetBytes(connectionString)));
                Properties.Settings.Default.Save();
                //MessageBox.Show("Conectado correctamente", "�xito");
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
                        dbConnectionError += "Usuario o contrase�a inv�lidos.";
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
                MessageBox.Show("Hubo un error al realizar la impresi�n, revisa que el nombre de la impresora sea el correcto.", "Error");
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

        private void resetHistory()
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    turnsLbl.Text = "Turno -";
                    this.turnsCounter = 1;
                    Properties.Settings.Default.CurrentTurn = this.turnsCounter;
                    dt.Clear();
                    Properties.Settings.Default.TableDataXML = "";
                    Properties.Settings.Default.Save();
                }));
            }
            else
            {
                turnsLbl.Text = "Turno -";
                this.turnsCounter = 1;
                Properties.Settings.Default.CurrentTurn = this.turnsCounter;
                dt.Clear();
                Properties.Settings.Default.TableDataXML = "";
                Properties.Settings.Default.Save();
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
