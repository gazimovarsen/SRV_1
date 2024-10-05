using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace SRV_1
{
    public partial class Form1 : Form
    {
        private const int bufferSize = 5;
        Queue<char> buffer1 = new Queue<char>();
        Queue<char> buffer2 = new Queue<char>();
        Random random = new Random();
        SemaphoreSlim semaphore = new SemaphoreSlim(0, bufferSize);
        bool running;

        Thread producerThread;
        Thread consumerThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                btnStart.Text = "Stop";
                running = true;
                producerThread = new Thread(Producer);
                consumerThread = new Thread(Consumer);

                producerThread.Start();
                consumerThread.Start();
            }
            else
            {
                btnStart.Text = "Start";
                Kill();
            }
        }

        void Producer()
        {
            while (running)
            {
                try
                {
                    char message = (char)('A' + random.Next(26));

                    if (buffer1.Count >= bufferSize)
                    {
                        buffer1.Dequeue();
                    }

                    buffer1.Enqueue(message);
                    UpdateBufferDisplay(buffer1, txtBuffer);
                    if (semaphore.CurrentCount < bufferSize)
                    {
                        semaphore.Release();
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    ShowException(e.Message);
                }
            }
        }

        void Consumer()
        {
            while (running)
            {
                try
                {
                    semaphore.Wait();

                    if (buffer1.Count > 0)
                    {
                        char message = buffer1.Dequeue();
                        buffer2.Enqueue(message);

                        UpdateResultsDisplay(message);
                    }

                    Thread.Sleep(3500);
                }
                catch (Exception e)
                {
                    ShowException(e.Message);
                }
            }
        }

        void UpdateBufferDisplay(Queue<char> buffer, TextBox textBox)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => UpdateBufferDisplay(buffer, textBox)));
            }
            else
            {
                textBox.Text = string.Join(Environment.NewLine, buffer.ToArray());
            }
        }

        void UpdateResultsDisplay(char message)
        {
            if (txtResults.InvokeRequired)
            {
                txtResults.Invoke(new Action(() => UpdateResultsDisplay(message)));
            }
            else
            {
                txtResults.AppendText($"Обработан {message}{Environment.NewLine}");
            }
        }

        void ShowException(string message)
        {
            txtResults.AppendText($"Обработан {message}{Environment.NewLine}");
        }

        private void Kill()
        {
            running = false;
            semaphore.Dispose();
            producerThread?.Join();
            consumerThread?.Join();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Kill();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Kill();
        }
    }
}
