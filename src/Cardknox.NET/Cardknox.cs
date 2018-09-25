﻿using CardknoxApi.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace CardknoxApi
{
    public class Cardknox
    {
        /// <summary>
        /// 
        /// </summary>
        private CardknoxRequest _request { get; }
        private NameValueCollection _values { get; }

        //public Sale Sale;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request">The <see cref="CardknoxRequest"/> object that is used to make the request.</param>
        public Cardknox(CardknoxRequest request)
        {
            _values = new NameValueCollection
            {
                { "xKey", request._key },
                { "xVersion", request._cardknoxVersion },
                { "xSoftwareName", request._software },
                { "xSoftwareVersion", request._softwareVersion }
            };
            _request = request;
        }

        #region credit card

        /// <summary>
        /// The Sale command is a combination of an authorization and capture and intended when fulfilling an order right away. For transactions that are not fulfilled right away, use the authonly command initially and use the capture command to complete the sale.
        /// </summary>
        /// <param name="_sale"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCSale(CCSale _sale, bool force = false)
        {
            if (_sale.Amount == null || _sale.Amount <= 0)
                throw new InvalidOperationException("Invalid amount. Sale Amount must be greater than 0.");
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _sale.Operation);
            _values.Add("xAmount", String.Format("{0:N2}", _sale.Amount));
            bool requiredAdded = false;
            // These groups are mutually exclusive
            if (!String.IsNullOrWhiteSpace(_sale.CardNum))
            {
                _values.Add("xCardNum", _sale.CardNum);
                _values.Add("xCVV", _sale.CVV);
                _values.Add("xExp", _sale.Exp);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_sale.Token))
            {
                _values.Add("xToken", _sale.Token);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_sale.MagStripe))
            {
                _values.Add("xMagStripe", _sale.MagStripe);
                requiredAdded = true;
            }
            if (!requiredAdded)
                throw new Exception("Missing required values. Please refer to the API documentation.");
            // END required information

            // The next many fields are optional and so there will be a lot of if statements here
            // Optional, but recommended
            if (!String.IsNullOrWhiteSpace(_sale.Street))
                _values.Add("xStreet", _sale.Street);

            if (!String.IsNullOrWhiteSpace(_sale.Zip))
                _values.Add("xZip", _sale.Zip);

            // IP is optional, but is highly recommended for fraud detection
            if (!String.IsNullOrWhiteSpace(_sale.IP))
                _values.Add("xIP", _sale.IP);

            if (!String.IsNullOrWhiteSpace(_sale.Invoice))
                _values.Add("xInvoice", _sale.Invoice);

            if (_sale.Tip != null)
                _values.Add("xTip", String.Format("{0:N2}", _sale.Tip));

            if (_sale.Tax != null)
                _values.Add("xTax", String.Format("{0:N2}", _sale.Tax));

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        /// <summary>
        /// The Save command is used to send account information and request a token from Cardknox, but does not submit the transaction for processing. The response returns a token which references that account information. A token at a minimum references the credit card number, but if other data is sent, such as billing address, that will be associated with the token as well.
        /// </summary>
        /// <param name="_save"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCSave(CCSave _save, bool force = false)
        {
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _save.Operation);
            bool requiredAdded = false;
            // These groups are mutually exclusive
            if (!String.IsNullOrWhiteSpace(_save.CardNum))
            {
                _values.Add("xCardNum", _save.CardNum);
                _values.Add("xCVV", _save.CVV);
                _values.Add("xExp", _save.Exp);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_save.Token))
            {
                _values.Add("xToken", _save.Token);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_save.MagStripe))
            {
                _values.Add("xMagStripe", _save.MagStripe);
                requiredAdded = true;
            }
            if (!requiredAdded)
                throw new Exception("Missing required values. Please refer to the API documentation.");
            // END required information

            // The next many fields are optional and so there will be a lot of if statements here
            // Optional, but recommended
            if (!String.IsNullOrWhiteSpace(_save.Name))
                _values.Add("xName", _save.Name);

            if (!String.IsNullOrWhiteSpace(_save.Street))
                _values.Add("xStreet", _save.Street);

            if (!String.IsNullOrWhiteSpace(_save.Zip))
                _values.Add("xZip", _save.Zip);

            // IP is optional, but is highly recommended for fraud detection
            if (!String.IsNullOrWhiteSpace(_save.IP))
                _values.Add("xIP", _save.IP);

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        /// <summary>
        /// The Refund command is used to refund a full or partial refund of a previous settled transaction, using RefNum.
        /// </summary>
        /// <param name="_refund"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCRefund(CCRefund _refund, bool force = false)
        {
            if (String.IsNullOrWhiteSpace(_refund.RefNum))
                throw new InvalidOperationException("Invalid RefNum specified. RefNum must reference a previous transaction.");
            if (_refund.Amount == null || _refund.Amount <= 0)
                throw new InvalidOperationException("Invalid amount. Must specify a positive amount to refund.");
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _refund.Operation);

            _values.Add("xAmount", String.Format("{0:N2}", _refund.Amount));
            _values.Add("xRefNum", _refund.RefNum);
            // END required information

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        /// <summary>
        /// The AuthOnly command authorizes an amount on a cardholder's account and places a hold on the available credit for that amount, but does not submit the charge for settlement. AuthOnly is used to reserve funds from a cardholder's credit limit for a sale that is not ready to be processed.
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCAuthOnly(CCAuthOnly _auth, bool force = false)
        {
            if (_auth.Amount == null || _auth.Amount <= 0)
                throw new InvalidOperationException("Invalid amount. Auth Amount must be greater than 0.");
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _auth.Operation);
            _values.Add("xAmount", String.Format("{0:N2}", _auth.Amount));
            bool requiredAdded = false;
            // These groups are mutually exclusive
            if (!String.IsNullOrWhiteSpace(_auth.CardNum))
            {
                _values.Add("xCardNum", _auth.CardNum);
                _values.Add("xCVV", _auth.CVV);
                _values.Add("xExp", _auth.Exp);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_auth.Token))
            {
                _values.Add("xToken", _auth.Token);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_auth.MagStripe))
            {
                _values.Add("xMagStripe", _auth.MagStripe);
                requiredAdded = true;
            }
            if (!requiredAdded)
                throw new Exception("Missing required values. Please refer to the API documentation.");
            // END required information

            // The next many fields are optional and so there will be a lot of if statements here
            // Optional, but recommended
            if (!String.IsNullOrWhiteSpace(_auth.Street))
                _values.Add("xStreet", _auth.Street);

            if (!String.IsNullOrWhiteSpace(_auth.Zip))
                _values.Add("xZip", _auth.Zip);

            // IP is optional, but is highly recommended for fraud detection
            if (!String.IsNullOrWhiteSpace(_auth.IP))
                _values.Add("xIP", _auth.IP);

            if (!String.IsNullOrWhiteSpace(_auth.Invoice))
                _values.Add("xInvoice", _auth.Invoice);

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        /// <summary>
        /// The Capture command is used to settle funds from a previous authorization and withdraw the funds from the cardholder's account. The refNumber from related authorization is required when submitting a Capture request. To perform an authorization and capture in the same command, use the CCSale command.
        /// </summary>
        /// <param name="_capture"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCCapture(CCCapture _capture, bool force = false)
        {
            if (String.IsNullOrWhiteSpace(_capture.RefNum))
                throw new InvalidOperationException("The capture command must reference a previous authorization in the RefNum parameter.");
            if (_capture.Amount == null || _capture.Amount <= 0)
                throw new InvalidOperationException("Invalid amount. Capture Amount must be greater than 0.");
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _capture.Operation);
            _values.Add("xAmount", String.Format("{0:N2}", _capture.Amount));
            _values.Add("xRefNum", _capture.RefNum);
            // END required information

            // The next many fields are optional and so there will be a lot of if statements here
            // Optional, but recommended
            if (!String.IsNullOrWhiteSpace(_capture.Street))
                _values.Add("xStreet", _capture.Street);

            if (!String.IsNullOrWhiteSpace(_capture.Zip))
                _values.Add("xZip", _capture.Zip);

            // IP is optional, but is highly recommended for fraud detection
            if (!String.IsNullOrWhiteSpace(_capture.IP))
                _values.Add("xIP", _capture.IP);

            if (!String.IsNullOrWhiteSpace(_capture.Invoice))
                _values.Add("xInvoice", _capture.Invoice);

            if (_capture.Tip != null)
                _values.Add("xTip", String.Format("{0:N2}", _capture.Tip));

            if (_capture.Tax != null)
                _values.Add("xTax", String.Format("{0:N2}", _capture.Tax));

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        /// <summary>
        /// The Credit command refunds money from a merchant to a cardholder's card that is not linked to any previous transaction.
        /// </summary>
        /// <param name="_credit"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCCredit(CCCredit _credit, bool force = false)
        {
            if (_credit.Amount == null || _credit.Amount <= 0)
                throw new InvalidOperationException("Invalid amount. Credit Amount must be greater than 0.");
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _credit.Operation);
            _values.Add("xAmount", String.Format("{0:N2}", _credit.Amount));
            bool requiredAdded = false;
            // These groups are mutually exclusive
            if (!String.IsNullOrWhiteSpace(_credit.CardNum))
            {
                _values.Add("xCardNum", _credit.CardNum);
                _values.Add("xCVV", _credit.CVV);
                _values.Add("xExp", _credit.Exp);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_credit.Token))
            {
                _values.Add("xToken", _credit.Token);
                requiredAdded = true;
            }
            else if (!String.IsNullOrWhiteSpace(_credit.MagStripe))
            {
                _values.Add("xMagStripe", _credit.MagStripe);
                requiredAdded = true;
            }
            if (!requiredAdded)
                throw new Exception("Missing required values. Please refer to the API documentation.");
            // END required information

            // The next many fields are optional and so there will be a lot of if statements here
            // Optional, but recommended
            if (!String.IsNullOrWhiteSpace(_credit.Street))
                _values.Add("xStreet", _credit.Street);

            if (!String.IsNullOrWhiteSpace(_credit.Zip))
                _values.Add("xZip", _credit.Zip);

            // IP is optional, but is highly recommended for fraud detection
            if (!String.IsNullOrWhiteSpace(_credit.IP))
                _values.Add("xIP", _credit.IP);

            if (!String.IsNullOrWhiteSpace(_credit.Invoice))
                _values.Add("xInvoice", _credit.Invoice);

            if (_credit.Tip != null)
                _values.Add("xTip", String.Format("{0:N2}", _credit.Tip));

            if (_credit.Tax != null)
                _values.Add("xTax", String.Format("{0:N2}", _credit.Tax));

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        /// <summary>
        /// The Void command voids a captured transaction that is pending batch, prior to the batch being settled.
        /// </summary>
        /// <param name="_void"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public CardknoxResponse CCVoid(CCVoid _void, bool force = false)
        {
            if (_values.AllKeys.Length > 4 && !force)
                throw new InvalidOperationException("A new instance of Cardknox is required to perform this operation unless 'force' is set to 'true'.");
            else if (force)
            {
                string[] toRemove = _values.AllKeys;
                foreach (var v in toRemove)
                    _values.Remove(v);
                _values.Add("xKey", _request._key);
                _values.Add("xVersion", _request._cardknoxVersion);
                _values.Add("xSoftwareName", _request._software);
                _values.Add("xSoftwareVersion", _request._softwareVersion);
            }

            // BEGIN required information
            _values.Add("xCommand", _void.Operation);

            if (String.IsNullOrWhiteSpace(_void.RefNum))
                throw new InvalidOperationException("Invalid RefNum specified. RefNum must reference a previous transaction.");

            _values.Add("xRefNum", _void.RefNum);
            // END required information

            var resp = MakeRequest();
            return new CardknoxResponse(resp);
        }

        #endregion

        private NameValueCollection MakeRequest()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient c = new WebClient();
            string req = System.Text.Encoding.ASCII.GetString(c.UploadValues(CardknoxRequest._url, _values));
            NameValueCollection resp = HttpUtility.ParseQueryString(req);

            return resp;
        }
    }
}
