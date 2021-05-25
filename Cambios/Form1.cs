

namespace Cambios
{
    using Cambios.Servicos;
    using Modelos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        #region Atributos

        private NetworkService networkService;

        private ApiService apiService;

        private List<Rate> Rates;

        private DialogService dialogService;

        #endregion

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            LoadRates();
        }

        private async void LoadRates()
        {

            //Load da API
            ProgressBar1.Value = 0;

            bool load;

            LabelResultado.Text = "A atualizar taxas...";

            var connection = networkService.CheckConnection();

            if(!connection.IsSuccess)
            {
                LoadLocalRates();
                load = false;
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                LabelResultado.Text = "Não há ligação á Internet" + Environment.NewLine +
                    "e não foram préviamente carregadas as taxas." + Environment.NewLine +
                    "Tente mais tarde!";

                return;
            }

            ComboBoxOrigem.DataSource = Rates;
            ComboBoxOrigem.DisplayMember = "Name";

            //Corrige bug da microsoft...
            ComboBoxDestino.BindingContext = new BindingContext();

            ComboBoxDestino.DataSource = Rates;
            ComboBoxDestino.DisplayMember = "Name";

            

            LabelResultado.Text = "Taxas atualizadas...";

            if (load)
            {
                LabelStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                LabelStatus.Text = string.Format("Taxas carregadas da Base de Dados.");
            }

            ProgressBar1.Value = 100;
            ButtonConverter.Enabled = true;
            ButtonTroca.Enabled = true;
        }

        private void LoadLocalRates()
        {
            MessageBox.Show("Não está implementado");
        }

        private async Task LoadApiRates()
        {
            ProgressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com","/api/rates");

            Rates = (List<Rate>) response.Result;
        }

        private void ButtonConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if(string.IsNullOrEmpty(TextBoxValor.Text))
            {
                dialogService.ShowMessage("Erro", "Insira um valor a converter");
                return;
            }

            decimal valor;

            if (!decimal.TryParse(TextBoxValor.Text, out valor))
            {
                dialogService.ShowMessage("Erro de conversão", "Valor terá que ser numérico");

            }

            if(ComboBoxOrigem.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda a converter");
                return;
            }

            if (ComboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda de destino para converter");
                return;
            }

            var taxaOrigem = (Rate) ComboBoxOrigem.SelectedItem;

            var taxaDestino = (Rate) ComboBoxDestino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            LabelResultado.Text = string.Format("{0} {1:C2} = {2} {3:C2}",
                taxaOrigem.Code,
                valor,
                taxaDestino.Code,
                valorConvertido);


        }

        private void ButtonTroca_Click(object sender, EventArgs e)
        {
            Troca();
        }

        private void Troca()
        {
            var aux = ComboBoxOrigem.SelectedItem;
            ComboBoxOrigem.SelectedItem = ComboBoxDestino.SelectedItem;
            ComboBoxDestino.SelectedItem = aux;
            Converter();
        }
    }
}
