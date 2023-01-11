using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace Crypto_Stats
{
    public partial class frmMain : Form
    {
        enum rows { TransactionType = 0, Date = 1, AmountDebited = 2, DebitCurrency = 3, AmountCredited = 4, CreditCurrency = 5, BuySellRate = 6, Direction = 7, SpotRate = 8, SourceDestination = 9, BlockchainTransactionID = 10 };
        public frmMain()
        {
            InitializeComponent();
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Stats File\\transactions_summary.csv";

            using (TextFieldParser tfp = new TextFieldParser(fileName))
            {
                tfp.SetDelimiters(",");
                if (!tfp.EndOfData)
                {
                    string[] fields = tfp.ReadFields();

                    for (int i = 0; i < fields.Count(); i++)
                    {
                        dgvTransactions.Columns.Add(fields[i], fields[i]);
                    }

                    while (!tfp.EndOfData)
                    {
                        dgvTransactions.Rows.Add(tfp.ReadFields());
                    }
                }
            }
            LoadStats();
            nudBtcPrice.Value = GetBtcPrice();
            nudEthPrice.Value = GetEthPrice();
            UpdateStats();
        }

        private void LoadStats()
        {
            double btcBought = 0;
            double btcBoughtInCad = 0;
            double btcSold = 0;
            double btcSoldInCad = 0;

            double ethBought = 0;
            double ethBoughtInCad = 0;
            double ethSold = 0;
            double ethSoldInCad = 0;

            int timesShaked = 0;
            double satsEarnings = 0;

            double cardCashBack = 0;
            double cardTransactions = 0;

            double funding = 0;
            double cashout = 0;
            double receivedCrypto = 0;
            double sentCrypto = 0;
            double receivedTransfer = 0;
            double sentTransfer = 0;
            double other = 0;

            foreach (DataGridViewRow row in dgvTransactions.Rows)
            {
                if (row.Cells[(int)rows.TransactionType].Value != null)
                {
                    string transactionType = row.Cells[(int)rows.TransactionType].Value.ToString();
                    if (transactionType.Equals("purchase/sale"))
                    {
                        string creditCurrency = row.Cells[(int)rows.CreditCurrency].Value.ToString();
                        string debitCurrency = row.Cells[(int)rows.DebitCurrency].Value.ToString();
                        double amountCredited = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                        double amountDebited = Convert.ToDouble(row.Cells[(int)rows.AmountDebited].Value.ToString().Replace(".", ","));
                        if (creditCurrency.Equals("BTC"))
                        {
                            btcBought += amountCredited;
                            btcBoughtInCad += amountDebited;
                        }
                        else if (creditCurrency.Equals("ETH"))
                        {
                            ethBought += amountCredited;
                            ethBoughtInCad += amountDebited;
                        }
                        else if (debitCurrency.Equals("BTC"))
                        {
                            btcSold += amountDebited;
                            btcSoldInCad += amountCredited;
                        }
                        else if (debitCurrency.Equals("ETH"))
                        {
                            ethSold += amountDebited;
                            ethSoldInCad += amountCredited;
                        }
                    }
                    else if (transactionType.Equals("shakingsats"))
                    {
                        double amountCredited = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                        timesShaked++;
                        satsEarnings += amountCredited * 100000000;
                    }
                    else if (transactionType.Equals("card cashbacks"))
                    {
                        double amountCredited = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                        cardCashBack += amountCredited;
                    }
                    else if (transactionType.Equals("card transactions"))
                    {
                        double amountDebited = Convert.ToDouble(row.Cells[(int)rows.AmountDebited].Value.ToString().Replace(".", ","));
                        cardTransactions += amountDebited;
                    }
                    else if (transactionType.Equals("fiat funding"))
                    {
                        double amountFund = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                        funding += amountFund;
                    }
                    else if (transactionType.Equals("fiat cashout"))
                    {
                        double amountCashOut = Convert.ToDouble(row.Cells[(int)rows.AmountDebited].Value.ToString().Replace(".", ","));
                        cashout += amountCashOut;
                    }
                    else if (transactionType.Equals("crypto cashout"))
                    {
                        double amountCashout = Convert.ToDouble(row.Cells[(int)rows.AmountDebited].Value.ToString().Replace(".", ","));
                        double cryptoRate = Convert.ToDouble(row.Cells[(int)rows.SpotRate].Value.ToString().Replace(".", ","));
                        sentCrypto += amountCashout * cryptoRate;
                    }
                    else if (transactionType.Equals("crypto funding"))
                    {
                        double amountFund = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                        double cryptoRate = Convert.ToDouble(row.Cells[(int)rows.SpotRate].Value.ToString().Replace(".", ","));
                        receivedCrypto += amountFund * cryptoRate;
                    }
                    else if (transactionType.Equals("peer transfer"))
                    {
                        if (row.Cells[(int)rows.Direction].Value.ToString() == "credit")
                        {
                            double amountReceived = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                            receivedTransfer += amountReceived;
                        }
                        else
                        {
                            double amountSent = Convert.ToDouble(row.Cells[(int)rows.AmountDebited].Value.ToString().Replace(".", ","));
                            sentTransfer += amountSent;
                        }
                    }
                    else if (transactionType.Equals("other"))
                    {
                        double amountOther = Convert.ToDouble(row.Cells[(int)rows.AmountCredited].Value.ToString().Replace(".", ","));
                        other += amountOther;
                    }
                }
            }

            lblFunding.Text = Math.Round(funding, 2).ToString();
            lblReceivedCryptoCAD.Text = Math.Round(receivedCrypto, 2).ToString();
            lblTransferReceived.Text = Math.Round(receivedTransfer, 2).ToString();
            lblInOther.Text = Math.Round(other, 2).ToString();
            lblTotalAdded.Text = Math.Round(funding + receivedCrypto + receivedTransfer + other, 2).ToString();

            lblCashout.Text = Math.Round(cashout, 2).ToString();
            lblSentCryptoCAD.Text = Math.Round(sentCrypto, 2).ToString();
            lblTransferSend.Text = Math.Round(sentTransfer, 2).ToString();
            lblBoughtWithCard.Text = Math.Round(cardTransactions, 2).ToString();
            lblTotalRemovedFromApp.Text = Math.Round(cashout + sentCrypto + sentTransfer + cardTransactions, 2).ToString();

            lblInMinusOut.Text = Math.Round((funding + receivedCrypto + receivedTransfer + other) - (cashout + sentCrypto + sentTransfer + cardTransactions), 2).ToString();

            lblBtcBought.Text =  Math.Round(btcBought, 8).ToString();
            lblBtcBoughtInCad.Text =  Math.Round(btcBoughtInCad,2).ToString();
            lblBtcSold.Text =  Math.Round(btcSold, 8).ToString();
            lblBtcSoldInCad.Text = Math.Round(btcSoldInCad,2).ToString();

            lblBtcStatus.Text = Math.Round(btcBought - btcSold, 8).ToString();
            lblBtcStatusInCad.Text = Math.Round(btcSoldInCad - btcBoughtInCad, 2).ToString();
            if (Convert.ToDouble(lblBtcStatusInCad.Text) < 0)
            {
                lblBtcStatusInCad.BackColor = Color.Red;
            }

            lblEthBought.Text =  Math.Round(ethBought, 8).ToString();
            lblEthBoughtInCad.Text = Math.Round(ethBoughtInCad,2).ToString();
            lblEthSold.Text =  Math.Round(ethSold, 8).ToString();
            lblEthSoldInCad.Text = Math.Round(ethSoldInCad,2).ToString();

            lblEthStatus.Text =  Math.Round(ethBought - ethSold, 8).ToString();
            lblEthStatusInCad.Text = Math.Round(ethSoldInCad - ethBoughtInCad,2).ToString();
            if (Convert.ToDouble(lblEthStatusInCad.Text) < 0)
            {
                lblEthStatusInCad.BackColor = Color.Red;
            }

            lblSatsTotal.Text = satsEarnings.ToString();
            lblSatsTotalBtc.Text = "("+ Math.Round(satsEarnings / 100000000,5) + " BTC)";
            lblSatsCount.Text = timesShaked.ToString();
            lblSatsAverage.Text = Math.Round(satsEarnings / timesShaked, 2).ToString() + " Sats";

            lblCardCashback.Text = Math.Round(cardCashBack*100000000, 2).ToString();
            lblCardSpend.Text = Math.Round(cardTransactions,2).ToString();

            lblProfitRaw.Text = Math.Round(Convert.ToDouble(lblBtcStatusInCad.Text) + Convert.ToDouble(lblEthStatusInCad.Text),2).ToString();
            if(Convert.ToDouble(lblProfitRaw.Text) < 0)
            {
                lblProfitRaw.BackColor = Color.Red;
            }
            else
            {
                lblProfitRaw.BackColor = Color.Green;
            }
            UpdateStats();
        }
        private void UpdateStats()
        {
            double btcEstimation = Convert.ToDouble(nudBtcPrice.Value);
            double btcStatusInCad = Convert.ToDouble(lblBtcStatusInCad.Text);
            double btcStatus = Convert.ToDouble(lblBtcStatus.Text);

            double ethEstimation = Convert.ToDouble(nudEthPrice.Value);
            double ethStatusInCad = Convert.ToDouble(lblEthStatusInCad.Text);
            double ethStatus = Convert.ToDouble(lblEthStatus.Text);
            
            double satsTotal = Convert.ToDouble(lblSatsTotal.Text);
            double satsCount = Convert.ToDouble(lblSatsCount.Text);
            double satsCashBack = Convert.ToDouble(lblCardCashback.Text);

            double cardSpend = Convert.ToDouble(lblCardSpend.Text);
            double cardOtherPercent = Convert.ToDouble(nudCardOtherPercent.Value);

            lblBtcEstimatedProfit.Text = Math.Round(btcStatusInCad + (btcStatus * btcEstimation), 2).ToString();
            if (Convert.ToDouble(lblBtcEstimatedProfit.Text.ToString()) < 0)
            {
                lblBtcEstimatedProfit.BackColor = Color.Red;
            }
            else { lblBtcEstimatedProfit.BackColor = Color.Green; }
            lblEthEstimatedProfit.Text = Math.Round(ethStatusInCad + (ethStatus * ethEstimation),2).ToString();
            if (Convert.ToDouble(lblEthEstimatedProfit.Text.ToString()) < 0)
            {
                lblEthEstimatedProfit.BackColor = Color.Red;
            }
            else { lblEthEstimatedProfit.BackColor = Color.Green; }

            lblSatsAverageInCad.Text = Math.Round(satsTotal/100000000 / satsCount * btcEstimation,2).ToString();
            lblSatsTotalInCad.Text = Math.Round(satsTotal / 100000000 * btcEstimation, 2).ToString();

            lblCardCashBackInCad.Text = Math.Round(satsCashBack/100000000 * Convert.ToDouble(nudBtcPrice.Value), 2).ToString();
            lblCardPercent.Text = Math.Round(satsCashBack * btcEstimation / cardSpend / 1000000, 1).ToString() + " %";
            lblCardOtherCachback.Text = Math.Round(cardOtherPercent / 100 * cardSpend, 2).ToString();
            lblCardProfit.Text = Math.Round(Convert.ToDouble(lblCardCashBackInCad.Text) - Convert.ToDouble(lblCardOtherCachback.Text), 2).ToString();
            if (Convert.ToDouble(lblCardProfit.Text) < 0)
            {
                lblCardProfit.BackColor = Color.Red;
            }
            else
            {
                lblCardProfit.BackColor= Color.Green;
            }

            lblProfitSell.Text = Math.Round(Convert.ToDouble(lblBtcEstimatedProfit.Text) + Convert.ToDouble(lblEthEstimatedProfit.Text), 2).ToString();
            if (Convert.ToDouble(lblProfitSell.Text) < 0)
            {
                lblProfitSell.BackColor = Color.Red;
            }
            else
            {
                lblProfitSell.BackColor= Color.Green;
            }
            lblProfitTotal.Text = Math.Round(Convert.ToDouble(lblBtcEstimatedProfit.Text) + Convert.ToDouble(lblEthEstimatedProfit.Text) + Convert.ToDouble(lblSatsTotalInCad.Text) + Convert.ToDouble(lblCardProfit.Text), 2).ToString();
            if (Convert.ToDouble(lblProfitTotal.Text) < 0)
            {
                lblProfitTotal.BackColor = Color.Red;
            }
            else
            {
                lblProfitTotal.BackColor = Color.Green;
            }
        }

        private decimal GetBtcPrice()
        {
            string uri = "https://api.coinbase.com/v2/prices/BTC-CAD/spot";

            WebClient client = new WebClient();
            client.UseDefaultCredentials = true;
            string data = client.DownloadString(uri);
            var jsonCrypto = JsonSerializer.Deserialize<Crypto>(data);
            double result = Convert.ToDouble(jsonCrypto.data.amount.ToString().Replace(".", ","));
            return Convert.ToDecimal(Math.Round(result, 2));
        }

        private decimal GetEthPrice()
        {
            string uri = "https://api.coinbase.com/v2/prices/ETH-CAD/spot";

            WebClient client = new WebClient();
            client.UseDefaultCredentials = true;
            string data = client.DownloadString(uri);
            var jsonCrypto = JsonSerializer.Deserialize<Crypto>(data);
            double result = Convert.ToDouble(jsonCrypto.data.amount.ToString().Replace(".", ","));
            return Convert.ToDecimal(Math.Round(result, 2));
        }
        private void nudBtcPrice_ValueChanged(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void nudEthPrice_ValueChanged(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void nudCardOtherPercent_ValueChanged(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void btnGetCurrentBtcPrice_Click(object sender, EventArgs e)
        {
            nudBtcPrice.Value = GetBtcPrice();
            UpdateStats();
        }

        private void btnGetCurrentEth_Click(object sender, EventArgs e)
        {
            nudEthPrice.Value = GetEthPrice();
            UpdateStats();
        }
    }
    public class Crypto
    {
        public Amount data { get; set; }

    }
    public class Amount
    {
        public string amount { get; set; }
    }
}