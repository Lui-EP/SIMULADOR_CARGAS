using System;                     // Funcionalidades básicas de C#
using System.Collections.Generic; // Colecciones como List<T>
using System.ComponentModel;      // Para componentes con propiedades y eventos
using System.Data;                // Acceso y manipulación de datos
using System.Drawing;             // Gráficos y dibujo en pantalla
using System.Drawing.Drawing2D;   // Gráficos avanzados 2D (antialiasing, etc.)
using System.Linq;                // Operaciones LINQ para consultas
using System.Text;                // Manipulación de texto
using System.Threading.Tasks;     // Operaciones asíncronas
using System.Windows.Forms;       // Interfaz gráfica de usuario
using Microsoft.VisualBasic;      // Funcionalidades de Visual Basic (para diálogos)

namespace SIMULADOR_CARGAS
{
    // INTERFAZ ARRASTRABLE - Define comportamiento para objetos que pueden moverse con el ratón
    // [KEYWORD: arrastre, interfaz, movimiento, drag]
    public interface IDraggable
    {
        PointF Position { get; set; }
    }

    // DEFINICIÓN DE CARGA ELÉCTRICA - Representa una carga eléctrica con valor, posición y color
    // [KEYWORD: carga eléctrica, carga, value, valor carga, nanocoulombs]
    public class Charge : IDraggable
    {
        public float Value { get; set; }   // Valor de la carga en nanocoulombs (nC)
        public PointF Position { get; set; }  // Posición en pantalla
        public Color Color { get; set; }   // Color de representación (rojo para positivo, azul para negativo)

        // Constructor de carga eléctrica
        // [KEYWORD: crear carga, instanciar carga]
        public Charge(float value, PointF position, Color color)
        {
            Value = value;
            Position = position;
            Color = color;
        }
    }

    // DEFINICIÓN DE SENSOR - Elemento que muestra el vector de campo eléctrico en una posición
    // [KEYWORD: sensor, sensor campo, medidor campo]
    public class Sensor : IDraggable
    {
        public PointF Position { get; set; }  // Posición en pantalla del sensor

        // Constructor de sensor
        // [KEYWORD: crear sensor, instanciar sensor]
        public Sensor(PointF position)
        {
            Position = position;
        }
    }

    public partial class Form1 : Form
    {
        // LISTAS DE ELEMENTOS - Almacenan todas las cargas y sensores en la simulación
        // [KEYWORD: lista cargas, lista sensores, elementos simulación]
        private List<Charge> charges = new List<Charge>();
        private List<Sensor> sensors = new List<Sensor>();

        // OPCIONES DE VISUALIZACIÓN - Controlan qué elementos se muestran en pantalla
        // [KEYWORD: visualización, opciones visuales, mostrar, ocultar]
        private bool showDirectionOnly = false;  // Mostrar solo dirección sin magnitud
        private bool showGrid = true;            // Mostrar cuadrícula de fondo
        private bool showValues = true;          // Mostrar valores numéricos
        private bool showElectricField = true;   // Mostrar vectores de campo eléctrico

        // VARIABLES DE ARRASTRE - Controlan el movimiento de objetos con el mouse
        // [KEYWORD: selección, arrastre, movimiento, seleccionar objeto, mover, posición ratón]
        private IDraggable selectedObject = null;  // Objeto actualmente seleccionado
        private PointF lastMousePos;               // Última posición del ratón

        // FACTORES DE ESCALA - Controlan la visualización del campo eléctrico
        // [KEYWORD: escala, intensidad campo, longitud flecha, escala campo]
        private float fieldScale = 1.0f;           // Factor de escala para el campo eléctrico
        private float sensorMaxArrowLength = 200f;  // Longitud máxima de flecha para sensores
        private float sensorMinArrowLength = 20f;  // Longitud mínima de flecha para sensores

        // CONFIGURACIÓN VISUAL - Colores y tamaños de los elementos gráficos
        // [KEYWORD: colores, aspecto visual, tamaños, apariencia]
        private readonly Color positiveColor = Color.Red;       // Color para cargas positivas
        private readonly Color negativeColor = Color.DeepSkyBlue;  // Color para cargas negativas
        private readonly Color sensorColor = Color.Yellow;      // Color para sensores
        private readonly Color gridColor = Color.FromArgb(50, 150, 150, 150);  // Color de la grilla
        private readonly Color fieldLineColor = Color.White;    // Color de líneas de campo
        private readonly Color arrowColor = Color.FromArgb(120, 200, 200, 200);  // Color de flechas
        private readonly int chargeRadius = 12;    // Radio de las cargas en píxeles
        private readonly int sensorRadius = 5;     // Radio de los sensores en píxeles
        private readonly int gridSpacing = 40;     // Espaciado de la grilla en píxeles
        private readonly int arrowGridSpacing = 40;  // Espaciado de flechas de campo

        // FUENTES DE TEXTO - Para etiquetas y valores
        // [KEYWORD: fuentes, texto, formato texto]
        private readonly Font labelFont = new Font("Arial", 10);
        private readonly Font valueFont = new Font("Arial", 8);
        private readonly StringFormat centerFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        // MENÚ CONTEXTUAL - Para acciones con clic derecho
        // [KEYWORD: menú contextual, clic derecho, acciones carga]
        private ContextMenuStrip contextMenu;

        // CONSTRUCTOR PRINCIPAL - Inicializa la aplicación
        // [KEYWORD: inicialización, arranque, carga inicial]
        public Form1()
        {
            // Habilitar doble buffer para evitar parpadeo en el renderizado
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.ClientSize = new Size(1000, 700);
            this.Text = "Simulador de Campo Eléctrico";

            // Configurar UI y eventos
            SetupUI();
            SetupContextMenu();

            // Cargar ejemplo inicial: dipolo eléctrico (carga positiva y negativa)
            // [KEYWORD: dipolo, ejemplo inicial, configuración inicial]
            charges.Add(new Charge(+1, new PointF(300, 350), positiveColor));
            charges.Add(new Charge(-1, new PointF(700, 350), negativeColor));

            // Agregar un sensor de demostración
            // [KEYWORD: sensor inicial, sensor demo]
            sensors.Add(new Sensor(new PointF(400, 400)));
        }

        // CONFIGURACIÓN DE MENÚ CONTEXTUAL - Define las opciones al hacer clic derecho
        // [KEYWORD: menú contextual, eliminar objeto, cambiar valor, clic derecho]
        private void SetupContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            // Opción para eliminar objeto
            // [KEYWORD: eliminar carga, eliminar sensor, borrar]
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Eliminar");
            deleteItem.Click += (s, e) => {
                if (selectedObject != null)
                {
                    if (selectedObject is Charge)
                        charges.Remove(selectedObject as Charge);
                    else if (selectedObject is Sensor)
                        sensors.Remove(selectedObject as Sensor);

                    selectedObject = null;
                    Invalidate();  // Solicitar redibujado
                }
            };

            // Opción para cambiar valor de carga
            // [KEYWORD: modificar carga, cambiar valor, editar carga]
            ToolStripMenuItem changeValueItem = new ToolStripMenuItem("Cambiar Valor");
            changeValueItem.Click += (s, e) => {
                if (selectedObject is Charge charge)
                {
                    // Mostrar cuadro de diálogo para ingresar nuevo valor
                    string input = Microsoft.VisualBasic.Interaction.InputBox(
                        "Ingrese el nuevo valor en nC:",
                        "Cambiar Valor",
                        charge.Value.ToString());

                    if (float.TryParse(input, out float newValue))
                    {
                        charge.Value = newValue;

                        // Actualizar color basado en el signo de la carga
                        charge.Color = newValue >= 0 ? positiveColor : negativeColor;

                        Invalidate();  // Solicitar redibujado
                    }
                }
            };

            contextMenu.Items.AddRange(new ToolStripItem[] { deleteItem, changeValueItem });
        }

        // CONFIGURACIÓN DE INTERFAZ - Crea todos los controles de la aplicación
        // [KEYWORD: interfaz usuario, controles, botones, panel control]
        private void SetupUI()
        {
            // Panel de control lateral
            // [KEYWORD: panel lateral, panel control, opciones]
            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Casillas de verificación para opciones
            // [KEYWORD: checkboxes, opciones visualización, toggle]
            CheckBox chkElectricField = new CheckBox
            {
                Text = "Campo eléctrico",
                ForeColor = Color.LightGreen,
                Checked = showElectricField,
                AutoSize = true,
                Location = new Point(10, 20)
            };

            CheckBox chkDirectionOnly = new CheckBox
            {
                Text = "Mostrar solo dirección",
                ForeColor = Color.White,
                Checked = showDirectionOnly,
                AutoSize = true,
                Location = new Point(10, 50)
            };

            CheckBox chkValues = new CheckBox
            {
                Text = "Valores",
                ForeColor = Color.White,
                Checked = showValues,
                AutoSize = true,
                Location = new Point(10, 80)
            };

            CheckBox chkGrid = new CheckBox
            {
                Text = "Grilla",
                ForeColor = Color.White,
                Checked = showGrid,
                AutoSize = true,
                Location = new Point(10, 110)
            };

            // Control deslizante para intensidad del campo
            // [KEYWORD: slider, intensidad campo, escala campo]
            Label lblFieldScale = new Label
            {
                Text = "Intensidad del campo:",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 150)
            };

            TrackBar trkFieldScale = new TrackBar
            {
                Minimum = 1,
                Maximum = 20,
                Value = 10,
                Width = 180,
                Location = new Point(10, 170)
            };

            // Panel inferior para controles de cargas
            // [KEYWORD: panel inferior, botones carga, agregar cargas]
            Panel chargePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Botones para agregar cargas y sensores
            // [KEYWORD: botón carga positiva, agregar carga positiva]
            Button btnAddPositive = new Button
            {
                Text = "+1 nC",
                BackColor = positiveColor,
                ForeColor = Color.White,
                Size = new Size(80, 30),
                Location = new Point(10, 15),
                FlatStyle = FlatStyle.Flat
            };

            // [KEYWORD: botón carga negativa, agregar carga negativa]
            Button btnAddNegative = new Button
            {
                Text = "-1 nC",
                BackColor = negativeColor,
                ForeColor = Color.White,
                Size = new Size(80, 30),
                Location = new Point(100, 15),
                FlatStyle = FlatStyle.Flat
            };

            // [KEYWORD: botón sensor, agregar sensor]
            Button btnAddSensor = new Button
            {
                Text = "Sensor",
                BackColor = sensorColor,
                ForeColor = Color.Black,
                Size = new Size(80, 30),
                Location = new Point(190, 15),
                FlatStyle = FlatStyle.Flat
            };

            // [KEYWORD: botón borrar, limpiar simulación]
            Button btnClearAll = new Button
            {
                Text = "Borrar Todo",
                BackColor = Color.DarkGray,
                ForeColor = Color.Black,
                Size = new Size(100, 30),
                Location = new Point(280, 15),
                FlatStyle = FlatStyle.Flat
            };

            // Botón de salir
            // [KEYWORD: botón salir, cerrar aplicación]
            Button btnExit = new Button
            {
                Text = "Salir",
                BackColor = Color.FromArgb(192, 0, 0),
                ForeColor = Color.White,
                Size = new Size(80, 30),
                Location = new Point(390, 15),
                FlatStyle = FlatStyle.Flat
            };

            // Manejadores de eventos para las casillas de verificación
            // [KEYWORD: eventos checkbox, cambiar visualización]
            chkElectricField.CheckedChanged += (s, e) => { showElectricField = chkElectricField.Checked; Invalidate(); };
            chkDirectionOnly.CheckedChanged += (s, e) => { showDirectionOnly = chkDirectionOnly.Checked; Invalidate(); };
            chkValues.CheckedChanged += (s, e) => { showValues = chkValues.Checked; Invalidate(); };
            chkGrid.CheckedChanged += (s, e) => { showGrid = chkGrid.Checked; Invalidate(); };
            trkFieldScale.ValueChanged += (s, e) => { fieldScale = trkFieldScale.Value / 10.0f; Invalidate(); };

            // Manejadores para botón de carga positiva
            // [KEYWORD: evento agregar carga positiva]
            btnAddPositive.Click += (s, e) =>
            {
                // Agregar nueva carga positiva en el centro de la pantalla
                charges.Add(new Charge(+1, new PointF(ClientSize.Width / 2, ClientSize.Height / 2), positiveColor));
                Invalidate();  // Solicitar redibujado
            };

            // Manejadores para botón de carga negativa
            // [KEYWORD: evento agregar carga negativa]
            btnAddNegative.Click += (s, e) =>
            {
                // Agregar nueva carga negativa en el centro de la pantalla
                charges.Add(new Charge(-1, new PointF(ClientSize.Width / 2, ClientSize.Height / 2), negativeColor));
                Invalidate();  // Solicitar redibujado
            };

            // Manejadores para botón de sensor
            // [KEYWORD: evento agregar sensor]
            btnAddSensor.Click += (s, e) =>
            {
                // Agregar nuevo sensor en el centro de la pantalla
                sensors.Add(new Sensor(new PointF(ClientSize.Width / 2, ClientSize.Height / 2)));
                Invalidate();  // Solicitar redibujado
            };

            // Manejadores para botón de borrar todo
            // [KEYWORD: evento borrar todo, limpiar simulación]
            btnClearAll.Click += (s, e) =>
            {
                // Mostrar diálogo de confirmación antes de borrar
                DialogResult result = MessageBox.Show(
                    "¿Seguro que desea eliminar todas las cargas y sensores?",
                    "Confirmar borrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    charges.Clear();
                    sensors.Clear();
                    selectedObject = null;
                    Invalidate();  // Solicitar redibujado
                }
            };

            // Manejador para el botón de salir
            // [KEYWORD: evento salir, cerrar programa]
            btnExit.Click += (s, e) =>
            {
                // Mostrar diálogo de confirmación antes de salir
                DialogResult result = MessageBox.Show(
                    "¿Seguro que desea salir del simulador?",
                    "Confirmar salida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Exit();  // Cerrar la aplicación
                }
            };

            // Agregar controles al panel lateral
            controlPanel.Controls.AddRange(new Control[] {
                chkElectricField, chkDirectionOnly, chkValues, chkGrid,
                lblFieldScale, trkFieldScale
            });

            // Agregar controles al panel inferior (incluyendo botón de salir)
            chargePanel.Controls.AddRange(new Control[] {
                btnAddPositive, btnAddNegative, btnAddSensor, btnClearAll, btnExit
            });

            // Agregar paneles al formulario
            this.Controls.AddRange(new Control[] { controlPanel, chargePanel });

            // Configurar eventos del formulario
            // [KEYWORD: eventos ratón, eventos teclado]
            this.MouseDown += Form1_MouseDown;    // Detectar clic del mouse
            this.MouseMove += Form1_MouseMove;    // Detectar movimiento del mouse
            this.MouseUp += Form1_MouseUp;        // Detectar liberación del clic
            this.Paint += Form1_Paint;            // Dibujar en pantalla
            this.Resize += (s, e) => Invalidate();  // Redibujar al cambiar tamaño
            this.KeyDown += Form1_KeyDown;        // Detectar teclas presionadas
        }

        // MANEJO DE TECLAS - Permite eliminar objetos con Delete
        // [KEYWORD: tecla delete, eliminar con teclado, evento teclado]
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Eliminar objeto seleccionado con la tecla Delete
            if (e.KeyCode == Keys.Delete && selectedObject != null)
            {
                if (selectedObject is Charge)
                    charges.Remove(selectedObject as Charge);
                else if (selectedObject is Sensor)
                    sensors.Remove(selectedObject as Sensor);

                selectedObject = null;
                Invalidate();  // Solicitar redibujado
            }
        }

        // MANEJO DE CLIC DE RATÓN - Detecta la selección de objetos
        // [KEYWORD: clic ratón, seleccionar objeto, detectar selección]
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;

            // Verificar si se hizo clic en una carga
            // [KEYWORD: detectar clic en carga]
            foreach (var charge in charges)
            {
                if (Distance(e.Location, charge.Position) <= chargeRadius * 1.5)
                {
                    selectedObject = charge;

                    // Mostrar menú contextual con el botón derecho
                    if (e.Button == MouseButtons.Right)
                    {
                        contextMenu.Show(this, e.Location);
                    }

                    return;
                }
            }

            // Verificar si se hizo clic en un sensor
            // [KEYWORD: detectar clic en sensor]
            foreach (var sensor in sensors)
            {
                if (Distance(e.Location, sensor.Position) <= sensorRadius * 2)
                {
                    selectedObject = sensor;

                    // Mostrar menú contextual con el botón derecho
                    if (e.Button == MouseButtons.Right)
                    {
                        contextMenu.Show(this, e.Location);
                    }

                    return;
                }
            }
        }

        // MANEJO DE MOVIMIENTO DEL RATÓN - Implementa el arrastre de objetos
        // [KEYWORD: arrastrar objeto, mover carga, mover sensor, drag]
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Si hay un objeto seleccionado y se está arrastrando (botón izquierdo)
            if (selectedObject != null && e.Button == MouseButtons.Left)
            {
                selectedObject.Position = e.Location;  // Actualizar posición al arrastrar
                Invalidate();  // Solicitar redibujado
            }
        }

        // MANEJO DE LIBERACIÓN DE BOTÓN DEL RATÓN - Finaliza el arrastre
        // [KEYWORD: soltar objeto, finalizar arrastre]
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                selectedObject = null;  // Liberar el objeto seleccionado
            }
        }

        // MÉTODO PRINCIPAL DE DIBUJO - Renderiza todos los elementos en pantalla
        // [KEYWORD: dibujar, renderizar, paint, actualizar pantalla]
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;  // Suavizar bordes
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;  // Mejorar texto

            // Dibujar la cuadrícula de fondo
            // [KEYWORD: dibujar grid, cuadrícula, grid]
            if (showGrid)
            {
                using (Pen gridPen = new Pen(gridColor, 0.5f))
                {
                    // Líneas verticales
                    for (int x = 0; x < ClientSize.Width; x += gridSpacing)
                    {
                        g.DrawLine(gridPen, x, 0, x, ClientSize.Height);
                    }

                    // Líneas horizontales
                    for (int y = 0; y < ClientSize.Height; y += gridSpacing)
                    {
                        g.DrawLine(gridPen, 0, y, ClientSize.Width, y);
                    }
                }
            }

            // Dibujar flechas de campo eléctrico en la rejilla
            // [KEYWORD: dibujar campo eléctrico, flechas campo, visualización campo]
            if (showElectricField)
            {
                DrawElectricFieldGrid(g);
            }

            // Dibujar campo eléctrico en los sensores
            // [KEYWORD: dibujar sensores, flecha sensor, vector sensor]
            foreach (var sensor in sensors)
            {
                // Calcular campo eléctrico en la posición del sensor
                PointF field = CalculateElectricField(sensor.Position);

                // Dibujar el punto del sensor
                g.FillEllipse(new SolidBrush(sensorColor),
                    sensor.Position.X - sensorRadius,
                    sensor.Position.Y - sensorRadius,
                    sensorRadius * 2,
                    sensorRadius * 2);

                // Verificar si el campo es no nulo
                if (field.X != 0 || field.Y != 0)
                {
                    // Calcular magnitud y dirección del campo
                    // [KEYWORD: magnitud campo, dirección campo, vector campo]
                    float fieldMagnitude = (float)Math.Sqrt(field.X * field.X + field.Y * field.Y);
                    PointF direction = new PointF(field.X / fieldMagnitude, field.Y / fieldMagnitude);

                    // Encontrar la carga más cercana y su información
                    // [KEYWORD: carga cercana, distancia carga]
                    float minDistance = float.MaxValue;
                    Charge closestCharge = null;

                    foreach (var charge in charges)
                    {
                        float distance = Distance(sensor.Position, charge.Position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestCharge = charge;
                        }
                    }

                    // Calcular longitud de flecha basada en la carga más cercana
                    // [KEYWORD: longitud flecha, escala flecha, tamaño vector]
                    float arrowLength = sensorMinArrowLength; // Valor por defecto

                    if (closestCharge != null)
                    {
                        // La longitud debe ser inversamente proporcional a la distancia
                        float baseLength = Math.Min(sensorMaxArrowLength,
                                                   sensorMaxArrowLength * 100f / (minDistance + 10f));


                        // Cargas negativas: flechas más cortas cuando están cerca
                        if (closestCharge.Value < 0)
                        {
                            // Cuanto más cerca de la carga negativa, más corta la flecha
                            float distanceFactor = Math.Min(1.0f, minDistance / 200f);
                            arrowLength = sensorMinArrowLength + (baseLength - sensorMinArrowLength) * distanceFactor;

                            // Limitar el tamaño mínimo para que sea visible
                            arrowLength = Math.Max(arrowLength, sensorMinArrowLength * 0.5f);
                        }
                        // Cargas positivas: flechas más largas cuando están cerca
                        else
                        {
                            arrowLength = baseLength;
                        }

                        // Aplicar factor de escala global
                        arrowLength *= fieldScale;
                    }

                    // Calcular punto final de la flecha
                    PointF endPoint = new PointF(
                        sensor.Position.X + direction.X * arrowLength,
                        sensor.Position.Y + direction.Y * arrowLength);

                    using (Pen arrowPen = new Pen(Color.Red, 2))
                    {
                        // Dibujar línea de la flecha
                        g.DrawLine(arrowPen, sensor.Position, endPoint);
                        // Dibujar punta de flecha
                        DrawArrowHead(g, arrowPen, sensor.Position, endPoint, 15);
                    }
                }
            }

            // Dibujar las cargas eléctricas
            // [KEYWORD: dibujar cargas, visualizar cargas]
            foreach (var charge in charges)
            {
                // Dibujar círculo de la carga
                using (Brush chargeBrush = new SolidBrush(charge.Color))
                {
                    g.FillEllipse(chargeBrush,
                        charge.Position.X - chargeRadius,
                        charge.Position.Y - chargeRadius,
                        chargeRadius * 2,
                        chargeRadius * 2);
                }

                // Dibujar símbolo de la carga (+ o -)
                g.DrawString(charge.Value > 0 ? "+" : "-",
                    new Font("Arial", 12, FontStyle.Bold),
                    Brushes.White,
                    charge.Position.X,
                    charge.Position.Y,
                    centerFormat);

                // Mostrar valor de la carga si está habilitado
                // [KEYWORD: mostrar valor carga, etiqueta valor]
                if (showValues)
                {
                    g.DrawString($"{Math.Abs(charge.Value)} nC",
                        valueFont,
                        Brushes.White,
                        charge.Position.X,
                        charge.Position.Y + chargeRadius + 5,
                        centerFormat);
                }

                // Resaltar carga seleccionada
                // [KEYWORD: resaltar selección, carga seleccionada]
                if (selectedObject == charge)
                {
                    using (Pen selectionPen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawEllipse(selectionPen,
                            charge.Position.X - chargeRadius - 3,
                            charge.Position.Y - chargeRadius - 3,
                            (chargeRadius + 3) * 2,
                            (chargeRadius + 3) * 2);
                    }
                }
            }

            // Resaltar sensor seleccionado
            // [KEYWORD: resaltar sensor, sensor seleccionado]
            foreach (var sensor in sensors)
            {
                if (selectedObject == sensor)
                {
                    using (Pen selectionPen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawEllipse(selectionPen,
                            sensor.Position.X - sensorRadius - 3,
                            sensor.Position.Y - sensorRadius - 3,
                            (sensorRadius + 3) * 2,
                            (sensorRadius + 3) * 2);
                    }
                }
            }
        }

        // VERIFICACIÓN DE CANCELACIÓN DE CAMPO - Detecta si el sensor está en un punto de equilibrio
        // [KEYWORD: cancelación campo, equilibrio campo, punto nulo]
        private bool IsBetweenPositiveCharges(PointF position)
        {
            // Contar cargas positivas
            int positiveChargesCount = charges.Count(c => c.Value > 0);

            // Si no hay al menos dos cargas positivas, no es posible tener cancelación
            if (positiveChargesCount < 2)
                return false;

            // Obtener solo las cargas positivas
            var positiveCharges = charges.Where(c => c.Value > 0).ToList();

            // Calcular los vectores de campo para cada carga positiva
            List<PointF> fields = new List<PointF>();

            foreach (var charge in positiveCharges)
            {
                // Vector desde la carga hasta el punto
                float dx = position.X - charge.Position.X;
                float dy = position.Y - charge.Position.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Evitar división por cero
                if (distance < 1e-10f) continue;

                // E = k * q / r² (magnitud del campo eléctrico)
                float magnitude = 9e9f * charge.Value * 1e-9f / (distance * distance);
                magnitude *= 1e7f;  // Ajuste para visualización

                // Dirección del campo (desde la carga si es positiva)
                fields.Add(new PointF(magnitude * dx / distance, magnitude * dy / distance));
            }

            // Si no hay suficientes campos calculados, no hay cancelación
            if (fields.Count < 2)
                return false;

            // Calcular el campo eléctrico resultante
            // [KEYWORD: campo resultante, suma vectorial]
         
            PointF resultantField = new PointF(0, 0);
            foreach (var field in fields)
            {
                resultantField.X += field.X;
                resultantField.Y += field.Y;
            }

            // Calcular la magnitud del campo resultante
            float resultantMagnitude = (float)Math.Sqrt(resultantField.X * resultantField.X + resultantField.Y * resultantField.Y);

            // Calcular el promedio de magnitudes individuales
            float avgMagnitude = 0;
            foreach (var field in fields)
            {
                avgMagnitude += (float)Math.Sqrt(field.X * field.X + field.Y * field.Y);
            }
            avgMagnitude /= fields.Count;

            // Si la magnitud resultante es muy pequeña comparada con el promedio, hay cancelación
            return resultantMagnitude < (avgMagnitude * 0.2f);
        }

        // Dibuja una rejilla de flechas mostrando el campo eléctrico en toda la pantalla
        private void DrawElectricFieldGrid(Graphics g)
        {
            if (charges.Count == 0) return;  // No dibujar si no hay cargas

            // Calcular número de flechas en cada dirección
            int xCount = ClientSize.Width / arrowGridSpacing;
            int yCount = ClientSize.Height / arrowGridSpacing;

            // Recorrer la rejilla
            for (int x = 0; x <= xCount; x++)
            {
                for (int y = 0; y <= yCount; y++)
                {
                    PointF position = new PointF(
                        x * arrowGridSpacing,
                        y * arrowGridSpacing);

                    // Omitir si está demasiado cerca de una carga
                    bool tooClose = false;
                    foreach (var charge in charges)
                    {
                        if (Distance(position, charge.Position) < chargeRadius * 2)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    // Verificar si la posición está en un punto de cancelación entre cargas positivas
                    bool isBetweenPositiveCharges = IsBetweenPositiveCharges(position);
                    if (isBetweenPositiveCharges) continue;  // No dibujar flecha si hay cancelación

                    // Calcular campo eléctrico en esta posición
                    PointF field = CalculateElectricField(position);
                    if (field.X == 0 && field.Y == 0) continue;  // No dibujar si no hay campo

                    // Calcular longitud de flecha basada en magnitud del campo
                    float length = (float)Math.Sqrt(field.X * field.X + field.Y * field.Y);
                    PointF direction = new PointF(field.X / length, field.Y / length);

                    // Escalar la longitud de flecha según la intensidad del campo
                    float maxArrowLength = arrowGridSpacing * 0.7f;
                    float minArrowLength = arrowGridSpacing * 0.2f;
                    float normalizedLength = Math.Min(1.0f, length / 50000f * fieldScale);
                    float arrowLength = minArrowLength + (maxArrowLength - minArrowLength) * normalizedLength;

                    // Asegurar que la flecha tenga una longitud mínima visible incluso cuando el campo es débil
                    if (arrowLength < arrowGridSpacing * 0.15f)
                        arrowLength = arrowGridSpacing * 0.15f;

                    // Cuando solo se muestra dirección, usar longitud fija
                    if (showDirectionOnly)
                        arrowLength = arrowGridSpacing * 0.5f;

                    // Calcular punto final de la flecha
                    PointF endPoint = new PointF(
                        position.X + direction.X * arrowLength,
                        position.Y + direction.Y * arrowLength);

                    // Dibujar flecha
                    using (Pen arrowPen = new Pen(arrowColor, 1f))
                    {
                        g.DrawLine(arrowPen, position, endPoint);

                        // Dibujar cabeza de flecha más pequeña para la rejilla
                        DrawArrowHead(g, arrowPen, position, endPoint, 6);
                    }
                }
            }
        }

        // Dibuja una cabeza de flecha en el punto final de una línea
        private void DrawArrowHead(Graphics g, Pen pen, PointF start, PointF end, float size)
        {
            // Calcular ángulo de la línea
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            // Puntos para la cabeza de flecha
            PointF[] arrowHead = new PointF[3];
            arrowHead[0] = end;  // Punta de la flecha

            // Dos puntos traseros de la flecha (con ángulo de 30 grados)
            arrowHead[1] = new PointF(
                end.X - size * (float)Math.Cos(angle - Math.PI / 6),
                end.Y - size * (float)Math.Sin(angle - Math.PI / 6));

            arrowHead[2] = new PointF(
                end.X - size * (float)Math.Cos(angle + Math.PI / 6),
                end.Y - size * (float)Math.Sin(angle + Math.PI / 6));

            // Dibujar la cabeza de flecha
            g.FillPolygon(new SolidBrush(pen.Color), arrowHead);
        }

        // Calcula el campo eléctrico en una posición, sumando la contribución de todas las cargas
        private PointF CalculateElectricField(PointF position)
        {
            PointF field = new PointF(0f, 0f);

            foreach (var charge in charges)
            {
                // Vector desde la carga hasta el punto
                float dx = position.X - charge.Position.X;
                float dy = position.Y - charge.Position.Y;
                float distanceSquared = dx * dx + dy * dy;
                float distance = (float)Math.Sqrt(distanceSquared);

                // Evitar división por cero si estamos justo en una carga
                if (distance < 1e-10f) continue;

                // E = k * q / r² (magnitud del campo eléctrico)
                // k = 9e9 N·m²/C², q en nanocoulombs (nC), convertir a coulombs (C)
                float magnitude = 9e9f * charge.Value * 1e-9f / distanceSquared;
                magnitude *= 1e7f;  // Factor de escala para visualización

                // Dirección del campo (alejándose de cargas positivas, acercándose a cargas negativas)
                field.X += magnitude * dx / distance;
                field.Y += magnitude * dy / distance;
            }

            return field;
        }

        // Calcula la distancia entre dos puntos
        private float Distance(PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}