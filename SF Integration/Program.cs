using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SF_Integration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string opportunityId = "0065i00000GNuANAA1";
            string personRecordTypeId = "012J300000008yJIAQ";
            try
            {
                SalesforceAPIClient client = CreateClient();
                await client.LoginAsync();

                string getAccountId = "SELECT AccountId FROM Opportunity WHERE Id='" + opportunityId + "'";
                // Call the QueryAsync method and print the result
                string queryResponse = await client.QueryAsync(getAccountId);
                // Parse JSON and extract AccountId
                var jsonObject = JObject.Parse(queryResponse);
                string accountId = jsonObject["records"]?[0]?["AccountId"]?.ToString();
                Console.WriteLine("AccountId");
                Console.WriteLine(accountId);
                // Define the PATCH request parameters
                string accountEndpoint = "sobjects/Account/"+ accountId; // Salesforce Account record URL
                string accountJSONBody = "{ \"RecordTypeId\": \""+ personRecordTypeId + "\" }";
                // Call the PatchAsync method and print the result
                var accountUpdateResponse = await client.PatchAsync(accountEndpoint, accountJSONBody);
                if (accountUpdateResponse.StatusCode == System.Net.HttpStatusCode.NoContent) // 204
                {
                    Console.WriteLine("Account Updated");
                    string getAllRelatedData = "SELECT Id, CreatedDate, Pending_Admission__c, Caller_First_Name__c, Caller_Last_Name__c, Caller_Email__c, Caller_Primary_Phone__c, Caller_Secondary_Phone__c, Relation_to_Client__c, Primary_Contact_First_Name__c, Primary_Contact_Last_Name__c, Primary_Relation_to_Client__c, Primary_Contact_Phone__c, Email_Address__c, Emergency_Contact_1__c, Secondary_Contact_First_Name__c, Secondary_Contact_Last_Name__c, Secondary_Relation_to_Client__c, Secondary_Contact_Phone__c, Secondary_Contact_Email__c, Substance_Use_History__c, Emergency_Contact_2__c, Subscriber_First_Name__c, Subscriber_Last_Name__c, Subscriber_DOB__c, Policy_Holder_Relationship__c, Marital_Status__c, Referral_Source_Account__c, Referral_Source_Account__r.Name, Referral_Source_Account__r.Business_Type__c, Presenting_Crisis_Description__c, VOB_Summary__c, AccountId, Account.FirstName, Account.LastName, Account.MiddleName, Account.Client_DOB__c, Account.Client_SSN__c, Account.Gender__c, Account.Legal_Sex__c, Account.Pronouns__c, Account.SmartCare_MRN__c, Account.PersonMailingStreet, Account.PersonMailingCity, Account.PersonMailingStateCode, Account.PersonMailingPostalCode, Account.PersonMobilePhone, Account.PersonEmail, Account.BillingStreet, Account.BillingCity, Account.BillingStateCode, Account.BillingPostalCode, Account.Phone, Account.HomePhoneIsPrimary__c, Account.HomePhoneDoNotContact__c, Account.HomePhoneDoNotLeaveMessage__c, Account.CellPhoneIsPrimary__c, Account.CellDoNotContact__c, Account.CellDoNo, Referring_Professional__c, Referring_Professional__r.Phone, Referring_Professional__r.FirstName, Referring_Professional__r.LastName, Referring_Professional__r.MailingStreet, Referring_Professional__r.MailingCity, Referring_Professional__r.MailingStateCode, Referring_Professional__r.MailingPostalCode, Referring_Professional__r.Email FROM Opportunity where id='" + opportunityId + "'";
                    string getAllRelatedDataResponse = await client.QueryAsync(getAllRelatedData);
                    Console.WriteLine("Salesforce getAllRelateddata Query Response:");
                    Console.WriteLine(getAllRelatedDataResponse);
                    string getTaskData = "SELECT Id, WhatId, ActivityDate, Status, Description, AccountId, CreatedDate, CreatedById, CreatedBy.SmartcareStaffId, CreatedBy.Name, LastModifiedDate, LastModifiedById, LastModifiedBy.SmartcareStaffId, LastModifiedBy.Name, OwnerId, Owner.SmartcareStaffId, Owner.Name FROM Task WHERE WhatId ='" + opportunityId + "' AND Status = 'Completed'";
                    string getTaskDataResponse = await client.QueryAsync(getTaskData);
                    Console.WriteLine("Salesforce getTaskDataResponse Query Response:");
                    Console.WriteLine(getTaskDataResponse);
                    // Define the PATCH request parameters
                    string opportunityEndpoint = "sobjects/Opportunity/" + opportunityId;
                    string opportunutyJSONBody = "{ \"Pending_Admission__c\": \"Successful\" }";
                    // Call the PatchAsync method and print the result
                    var opportunityUpdateResponse = await client.PatchAsync(opportunityEndpoint, opportunutyJSONBody);
                    if (opportunityUpdateResponse.StatusCode == System.Net.HttpStatusCode.NoContent) // 204
                    {
                        Console.WriteLine("Opportunity Updated");
                    }
                    else
                    {
                        string opportunityResponseContent = await opportunityUpdateResponse.Content.ReadAsStringAsync();
                        Console.WriteLine("PATCH Response:");
                        Console.WriteLine(opportunityResponseContent);
                        Console.WriteLine("response.StatusCode:");
                        Console.WriteLine(opportunityUpdateResponse.StatusCode);
                    }
                }
                else
                {
                    string responseContent = await accountUpdateResponse.Content.ReadAsStringAsync();
                    Console.WriteLine("PATCH Response:");
                    Console.WriteLine(responseContent);
                    Console.WriteLine("response.StatusCode:");
                    Console.WriteLine(accountUpdateResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }

        private static SalesforceAPIClient CreateClient()
        {
            return new SalesforceAPIClient
            {
                Username = ConfigurationManager.AppSettings["username"],
                Password = ConfigurationManager.AppSettings["password"],
                Token = ConfigurationManager.AppSettings["token"],
                ClientId = ConfigurationManager.AppSettings["clientId"],
                ClientSecret = ConfigurationManager.AppSettings["clientSecret"]
            };
        }
    }
}
