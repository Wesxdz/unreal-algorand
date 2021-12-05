using System;
using System.Drawing;
using System.Collections.Generic;

using Algorand;
using Algorand.V2;
using Algorand.Client;
using Algorand.V2.Model;
using Account = Algorand.Account;
using System.Text;
using System.Threading.Tasks;

using UnrealEngine.Framework;

// To update C# assembly run
// dotnet publish "../Framework" --configuration Debug --framework net5.0 && dotnet publish --configuration Debug --framework net5.0 --output "../../../../../Managed/SDK"
// dotnet publish "../Framework" --configuration Release --framework net5.0 && dotnet publish --configuration Release --framework net5.0 --output "../../../../../Managed/SDK"
// IF YOU UPDATE THIS CLASS, RUN THE ABOVE COMMAND
namespace Game {

	public class AccountUpdate
	{
		public Task<Algorand.V2.Model.Account> task;
	}
	public class System
	{ // Custom class for loading functions from blueprints
		static string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/ps2";
		static string ALGOD_API_TOKEN = "siylNuUp0xZppYCsXbiT9OwmETC6BtRRpXdNPN30";

		static AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
		static Dictionary<String, Account> accounts = new Dictionary<string, Account>();

		static Dictionary<String, List<AccountUpdate>> accountUpdateQueue = new Dictionary<String, List<AccountUpdate>>();
		public static void CreateAccount(ObjectReference account) {
			var accountActor = account.ToActor<Actor>();
			Algorand.Account testAccount = new Algorand.Account();
			accountActor.SetString("Address", testAccount.Address.ToString());
			string mnemonic = testAccount.ToMnemonic();
			accountActor.SetText("Mnemonic", mnemonic);
			accounts[testAccount.Address.ToString()] = new Account(mnemonic);
		}

		public static void LoadAccount(ObjectReference account) {
			var accountActor = account.ToActor<Actor>();

			// https://en.wikipedia.org/wiki/Serenity_Prayer
			// https://github.com/nxrighthere/UnrealCLR/issues/216
			string m1 = null;
			accountActor.GetString("M1", ref m1);
			string m2 = null;
			accountActor.GetString("M2", ref m2);
			string mnemonic = m1 + m2;
			try {
				Account loadAccount = new Account(mnemonic);
				accountActor.SetString("Address", loadAccount.Address.ToString());
				accounts[loadAccount.Address.ToString()] = loadAccount;
				var accountInfo = algodApiInstance.AccountInformation(loadAccount.Address.ToString());
				if (accountInfo.Amount.HasValue)
				{
					Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, string.Format("Account Balance: {0} microAlgos", accountInfo.Amount));
					Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, "Account has value!");
					accountActor.SetFloat("Algo", accountInfo.Amount.Value/1000000.0f);
				} else
				{
					Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, "Account has no value :(");
				}
				foreach (AssetHolding holding in accountInfo.Assets)
				{
					var asset = algodApiInstance.GetAssetByID(holding.AssetId);
					accountActor.Invoke($"CreateAsset \"{asset.Params.Name}\" {holding.Amount}");
				}
			} finally
			{
				Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, "Failed to create account");
			}
		}

		public static void UpdateAccount(ObjectReference account)
		{
			var accountActor = account.ToActor<Actor>();
			string address = null;
			accountActor.GetString("Address", ref address);
			accounts[address] = new Account(accounts[address].ToMnemonic());
			if (!accountUpdateQueue.ContainsKey(address))
			{
				accountUpdateQueue[address] = new List<AccountUpdate>();
			}
			for (int i = accountUpdateQueue[address].Count - 1; i >= 0; i--)
			{
				AccountUpdate update = accountUpdateQueue[address][i];
				if (update.task.IsCompletedSuccessfully)
				{
					var asyncAccountInfo = update.task.Result;
					accountActor.SetFloat("Algo", asyncAccountInfo.Amount.Value/1000000.0f);
					accountUpdateQueue[address].RemoveRange(0, i + 1);
					break;
				}
			}
			var t = algodApiInstance.AccountInformationAsync(address);
			accountUpdateQueue[address].Add(new AccountUpdate{task=t});
		}

		public static void SendTransaction(ObjectReference transaction)
		{
			var transactionActor = transaction.ToActor<Actor>();
			string from = "";
			transactionActor.GetString("From", ref from);
			float amount = 0.0f;
			transactionActor.GetFloat("Amount", ref amount);
			string to = "";
			transactionActor.GetString("To", ref to);

            try
            {
                var trans = algodApiInstance.TransactionParams();
                var lr = trans.LastRound;
                var block = algodApiInstance.GetBlock(lr);

                Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Lastround: " + trans.LastRound.ToString());
                Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Block txns: " + block.Block.ToString());
            }
            catch (ApiException e)
            {
                Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Exception when calling algod#getSupply:" + e.Message);
            }

            TransactionParametersResponse transParams;
            try
            {
                transParams = algodApiInstance.TransactionParams();
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }

			string payMsg = ";)";
			Account src = accounts[from];
			Address dest = new Address(to);
            var tx = Utils.GetPaymentTransaction(accounts[from].Address, dest, Utils.AlgosToMicroalgos(amount), payMsg, transParams);
            var signedTx = src.SignTransaction(tx);


            Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Signed transaction with txid: " + signedTx.transactionID);

            // // send the transaction to the network
            try
            {
                var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(algodApiInstance, id.TxId);
                Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Confirmed Round is: " + resp.ConfirmedRound);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink,"Exception when calling algod#rawTransaction: " + e.Message);
            }

			Debug.AddOnScreenMessage(-1, 10.0f, Color.DeepPink, "Transacted");

		}
	}
}