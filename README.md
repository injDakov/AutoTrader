# AutoTrader Application

[![License](http://img.shields.io/:license-MIT-blue.svg)](https://licenses.nuget.org/MIT)

The **AutoTrader application** is an automation service that places crypto orders from the "Exchange Limit" type according to predefined rules. At regular time intervals, it checks that some orders are still active or are executed and place reciprocal orders with the same amount. 

**Example:** If we have an order of type "Sale" of 0.01 BTC and it is executed, then the AutoTrader will places an order for buying the same quantity of BTC at a price calculated by a coefficient **BuyLevel**.
And after some time when that order of type "Buy" was executed, the AutoTrader will place an order again for the amount of 0.01 BTC to sell at a price calculated by a coefficient **SellLevel**.

The profit will not be reinvested automatically and you can monitor it and reinvest it manually or withdraw it.

As a programmer and crypto enthusiast, I developed and still improving this application and keep it free and open-source. 
I believe that if we share our knowledge with people, the World will be a better place. 

The main purpose of creating this app is to meet my ideas for some small passive income.

`Showing this app is not a recommendation to join in the Crypto World or new investments. The application is "AS IS" and I am not responsible for any losses caused by the app or from the crypto bear market.`

**But for your profit, I will accept all the blame! :)**\
**And if you want, don't hesitate to grab me a beer.**

#### An expression of gratitude
For each person who donates, I can allocate space for his name, project, or other information that he wants to share with my auditory.
The data will be in the section **Friends and Supporters**.
This also can be ads exchange. :)

#### Donation opportunities
All wallets are provided and supported by [Bitfinex](https://bitfinex.com/?refcode=kwqvP9OMT). Please read the actual information.

* **Bitcoin (BTC)**\
Address - **3FqXqTzGhzxosERosz832F7rGDXMteMjRh**

* **Ethereum (ETH)** At this time Bitfinex does not accept transactions sent from smart contracts.\
Address - **0x818E07acD7d75d1812B2E7067e417C5Bc80d327E**

* **XRP (XRP)** Sending XRP requires both an address and a Tag\
Address - **rLW9gnQo7BQhU6igk5keqYnH3TVrCxGRzm**\
Tag - **2345006832**

* **Dogecoin (DOGE)**\
Address - **DNXrvtmrSWVyBKfmPizXQsreqLSRJQpqCv**

* **Bitcoin Cash Node (BCHN)** Depositing anything other than BCH Node WILL RESULT IN LOSS OF FUNDS. Bitfinex currently is not supporting automated chain splits.\
Address - **bitcoincash:qz4tu69cn50gryhdahemytrszdtnn9vwtqexclhleq**

* **Litecoin (LTC)**\
Address - **MD7ngCjhUbLNZfnvLdXkVu1LUkXwZMTHmF**

* **Monero (XMR)**\
Address - **86GNkvtYzbEZwq5MxpVvzP7kZ2tUgea5SKy8FTJ4S48fY8hz3DYBPWwJsLNz2woi7dQi34TYkcPDpZKh7iZLaaMLGHrSTwZ**

**Note:** It is recommended that you check the transaction fees before making a transaction and then select the appropriate currency. Sometimes the fees are extremely high.

# My experience with the AutoTrader

At the end of December 2020, I launched the app with some amount in USD distributed in 36 pairs and 61 active orders.

For these 7 months, the AutoTrader realized almost 1800 executed orders.
This is exactly a 32% profit up from my initial investment.

`But this is strongly related to the Crypto market volatility.`

Honestly, around 500 of these executed orders were in January.
The AutoTrader could not adapt to the prices with the fast increase in the market and many of the orders stayed under the current market price.

**But doesn't matter, 32% profit without additional work, is a 32% profit! :)**

# How to use it

1. Download the AutoTrader source code and spend time reviewing it. **Don't believe me at 100%! :)**
2. Carefully execute the steps in **Prerequisites** 
3. Carefully execute the steps in **How to configure the settings** 
4. Prepare the Publish files from the project (via the Visual Studio or with commands)
5. Install the AutoTrader as a normal Window Service
6. Be patient and enjoy
7. **Don't forget to grab me a beer if you like and appreciate my work :)**

**Note:** Basic IT knowledge is expected.

# Prerequisites 

* You have to install MS SQL Server ( The latest free SQL Express is good enough - [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) )
* [NON-MANDATORY] SMTP Server ( I am using the free by Google [Info]( https://support.google.com/a/answer/176600) )
* You must have an account in [Bitfinex](https://bitfinex.com/?refcode=kwqvP9OMT) ( If you are new I will appreciate it if you use my ref link! )
* You have to set up your **Key** and **Secret** in Bitfinex ( [How to do](https://support.bitfinex.com/hc/en-us/articles/115002349625-API-Key-Setup-Login))
* [HINT] I am using the Windows Server VM into [AWS Free Tier](https://aws.amazon.com/free/).  `It is completely free for 12 months but you should be careful when you create the VM because any wrong action can cost money!`
* [`STRONG SUGGEST`] Good idea is to use a [Bitfinex](https://bitfinex.com/?refcode=kwqvP9OMT) account that has only funds you want the AutoTrader to operate with them.

# How to configure the settings (appSettings.json)

#### MS SQL Server 
```
  "SqlSettings": {
    "ConnectionString": "Data Source=<Set your server address>; Initial Catalog=AutoTrader; User Id=<Set your username>; Password=<Set your password>;"
  }
```

#### SMTP Settings / Notifications via email
```
  "SmtpServerSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "<Set your username>",
    "Password": "<Set your password>",
    "Recipients": "<Set your recipients>",
    "IsActive": true
  }
```

#### Bitfinex API keys
```
  "BitfinexClient": {
    "Key": "<Set your key>",
    "Secret": "<Set your secret>"
  }
```

#### Profit percent
After 7 months **Live** and reviewing my own statistics I selected these ratios.
You can use them or set your own.

**`But keep in mind the logic: SellLevel > 0 and BuyLevel < 0 !`**

```
  "ProfitPercents": {
    "SellLevel1": 1.0115,
    "BuyLevel1": 0.986,
    "SellLevel2": 1.021,
    "BuyLevel2": 0.9765,
    "SellLevel3": 1.0295,
    "BuyLevel3": 0.967,
    "SellLevel4": 1.0375,
    "BuyLevel4": 0.957
  },
```

#### Pairs
As I mentioned at the beginning the AutoTrader application and this article are not recommendations for joining the Crypto market.\
For this consideration, I will **not share in a public place** my coin pairs.
Just shared the template and structure for how you can fill the configuration. 

The pairs name you can get from Bitfinex for free.

```
"Pairs": [
    {
      "Name": "BTCUSD",
      "OrderAmount": 0.01,
      "MaxOrderLevel": 3,
      "IsActive": "true"
    },
    {
      "Name": "ETHUSD",
      "OrderAmount": 0.1,
      "MaxOrderLevel": 1,
      "IsActive": "true"
    },
    {
      "Name": "LTCUSD",
      "OrderAmount": 1,
      "MaxOrderLevel": 2,
      "IsActive": "true"
    }
  ]
```

# Notifications

* **Healthcheck** - on each hour is sending an email with the count of active pairs and active orders
* **Trade event** - for each executed order and placed new order it is sending an email with information about that
* **Other** - In case of some error or other type of event that is not included in the above two types the AutoTrader is sending an email with information

# Friends and Supporters

Waiting for the first friends! :) 

# Q&A Section

**Q: Is that possible to lose funds with crypto investments?**\
**A:** The crypto market is high risk and volatile. You should be ready to gather huge losses in a short time but if you are patient you will earn in long term! Don't panic! :)

**Q: Is that possible to lost funds fault of the AutoTrader?**\
**A:** For these 7 months on Live with my real money there was no case to lost funds fault of the AutoTrader. But keep in mind that the application is "AS IS" and I am not responsible for any losses caused by the app or from the crypto bear market. Also, the AutoTrader is open source and you can get familiar with the uses algorithms. **If you found a bug contact me and I will try to fix it ASAP.**

**Q: Why in some places the documentation is missing or is not clear?**\
**A:** I am not pretending that I provided completely full documentation. If you have any questions and/or suggestions don't hesitate to contact me and I will answer you and will extend the documentation and Q&A section. **The easiest way is via message in [LinkedIn](https://bg.linkedin.com/in/dakov)**

# To Do List

* Add release note for each new version
* Improve the algorithm and bug fixing
* Move all configuration settings from the executable code to the appSettings
* Extend the Email notifications and make them more human-readable
* Add Binance as a second crypto trader platform

# Author

#### Petar Dakov

* [LinkedIn](https://bg.linkedin.com/in/dakov)
* [Personal Website](http://dakov.net/)

# License

[![License](http://img.shields.io/:license-MIT-blue.svg)](https://licenses.nuget.org/MIT)