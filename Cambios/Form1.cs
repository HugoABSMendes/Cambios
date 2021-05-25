

namespace Cambios
{
    using Cambios.Servicos;
    using Modelos;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        #region Atributos

        private NetworkService networkService;

        private ApiService apiService;



        #endregion

        public List<Rate> Rates { get; set; } = new List<Rate>();

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            LoadRates();
        }

        private async void LoadRates()
        {

            //Load da API
            //bool load;

            LabelResultado.Text = "A atualizar taxas...";

            var connection = networkService.CheckConnection();

            if(!connection.IsSuccess)
            {
                
                MessageBox.Show(connection.Message);
                return;
            }
            else
            {
                await LoadApiRates();
            }

            ProgressBar1.Value = 0;

            

            ComboBoxOrigem.DataSource = Rates;
            ComboBoxOrigem.DisplayMember = "Name";

            //Corrige bug da microsoft...
            ComboBoxDestino.BindingContext = new BindingContext();

            ComboBoxDestino.DataSource = Rates;
            ComboBoxDestino.DisplayMember = "Name";

            ProgressBar1.Value = 100;

            LabelResultado.Text = "Taxas carregadas...";
        }

        private async Task LoadApiRates()
        {
            ProgressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com","/api/rates");

            Rates = (List<Rate>) response.Result;
        }
    }
}
