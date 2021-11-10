# AutoTrader Application

[![License](http://img.shields.io/:license-MIT-blue.svg)](https://licenses.nuget.org/MIT)

The **AutoTrader application** is an automation service that places crypto orders from the "Exchange Limit" type. It checks that some orders are still active or are executed and place reciprocal orders with the same amount at regular time intervals.

**Example:** If we have an order of type "Sale" of 0.01 BTC and it is executed, then the AutoTrader will place an order for buying the same quantity of BTC at a price calculated by a coefficient **BuyLevel**.
And after some time when that order of type "Buy" was executed, the AutoTrader will place an order again for the amount of 0.01 BTC to sell at a price calculated by a coefficient **SellLevel**.

The profit will not be reinvested automatically and you can monitor it and reinvest it manually or withdraw it.

As a programmer and crypto enthusiast, I developed and still improving this application and keeping it free and open-source. 
I believe that if we share our knowledge with people, the World will be a better place. 

The main purpose of creating this app is to meet my ideas for passive income, with minimal monitoring time of the App.

`Showing this app is not a recommendation to join in the Crypto World or new investments. The application is "AS IS" and I am not responsible for any losses caused by the App or from the crypto bear market.`

`You don't need to send any funds to me or another crypto account. All of your funds will be in your personal account and under your control.`

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

At the end of December 2020, I launched the App with some amount in USD distributed in 36 pairs and 61 active orders.
Almost a year later (November 2021), from time to time I reinvested the profit and added a few fresh funds and now I have 37 active pairs with 99 orders.

In the first 7 months, the AutoTrader realized almost 1800 executed orders, this is exactly a 32% profit up from my initial investment.
For 11 months of uptime, I have over 2370 executed orders. The general profit for this working time is 48,7% profit up from my all deposits in the time.

**The 48,7% profit for less than a year, with just a few hours of support and price monitoring, is fair enough!**

The month with the best volume of executed orders was January, followed by August, September, October, and November.
The AutoTrader could not adapt to the prices with the fast increase in the market and part of the orders stayed under the current market price.

`But this is strongly related to the Crypto market volatility and your setting in the Pair config!`

In the current version, the trading algorithm is a bit improved and is more adaptive to the market levels.

My conclusion after monitoring the crypto market over the past few months is that it is better to focus on a few pairs with a bigger count of active orders. Thus we will have more executed orders, respectively more frequent profit.\
In the next days, I also will reduce the count of active pairs and will increase the count of active orders for selected pairs.

# How to use it

1. Download the AutoTrader source code and spend time reviewing it. **Don't believe me at 100%! :)**
2. Carefully execute the steps in **Prerequisites** 
3. Carefully execute the steps in **How to configure the settings** 
4. Prepare the Publish files from the project (via the Visual Studio or with commands)
5. Install the AutoTrader as a normal Window Service
6. Be patient and enjoy
7. **Don't forget to grab me a beer if you like and appreciate my work :)**

**Hint:** You can skip steps 4 and 5 and just start the AutoTrader (via the Visual Studio or with commands).

**Note:** Basic IT knowledge is expected.

# Prerequisites 

* You have to install MS SQL Server ( The latest free SQL Express is good enough - [Download](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) )
* Non-Mandatory: SMTP Server ( [I am using the free by Google]( https://support.google.com/a/answer/176600) )
* You must have an account in [Bitfinex](https://bitfinex.com/?refcode=kwqvP9OMT) ( If you are new I will appreciate it if you use my ref link! )
* You have to set up your **Key** and **Secret** in Bitfinex ( [How to do](https://support.bitfinex.com/hc/en-us/articles/115002349625-API-Key-Setup-Login))
* **HINT:** I am using the Windows Server VM into [AWS Free Tier](https://aws.amazon.com/free/).  `It is completely free for 12 months but you should be careful when you create the VM because any wrong action can cost money!`
* **HINT:** Instead of MS SQL Server, you can use [Amazon RDS Free Tier](https://aws.amazon.com/rds/free/)
* **STRONG SUGGEST:** Good approach is to create and use a second your [Bitfinex](https://bitfinex.com/?refcode=kwqvP9OMT) account that has only funds you want the AutoTrader to operate with them. I also use this approach.

# How to configure the settings (appSettings.json)

#### MS SQL Server 
```
"SqlSettings": {
    "ConnectionString": "Data Source=<Set your server address>; Initial Catalog=AutoTrader; User Id=<Set your username>; Password=<Set your password>;"
},
```

#### SMTP Settings / Notifications via email
```
"SmtpServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "<Set your username>",
    "Password": "<Set your password>",
    "Recipients": "<Set your recipients>",
    "IsActive": true
},
```

#### Bitfinex API keys
```
"BitfinexClient": {
    "Key": "<Set your key>",
    "Secret": "<Set your secret>"
},
```

#### Profit percents
After 11 months on **Live** and reviewing my own statistics I selected these ratios.
You can use them or set your own, or extend the lists.

**But keep in mind the logic: The elements count in both lists should be equal !**

```
"ProfitPercents": {
    "SellLevels": [ 1.0115, 1.021, 1.0295, 1.031, 1.0325 ],
    "BuyLevels": [ 0.986, 0.9765, 0.967, 0.9655, 0.964 ]
},
```

#### Additional settings
You can keep these values as is, or get familiar with the code and change them according to your opinion.

```
"HealthCheckIntervalInHours": 3,
"PricesQueueSize": 2,
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

* **Healthcheck** - on an interval of 'HealthCheckIntervalInHours' is sending an email with the count of active pairs and active orders
* **Trade event** - for each executed order and placed new order it is sending an email with information about that
* **Other** - In case of some error or other type of event that is not included in the above two types the AutoTrader is sending an email with information

# Friends and Supporters

Waiting for the first friends! :) 

# Q&A Section

**Q: Is that possible to lose funds with crypto investments?**\
**A:** The crypto market is high risk and volatile. You should be ready to gather huge losses in a short time but if you are patient you will earn in long term! Don't panic! :)

**Q: Is that possible to lose funds fault of the AutoTrader?**\
**A:** For almost one year on Live with my real money there was no case to lost funds fault of the AutoTrader. But keep in mind that the application is "AS IS" and I am not responsible for any losses caused by the App or from the crypto bear market. Also, the AutoTrader is open source and you can get familiar with the used algorithms. **If you found a bug contact me and I will try to fix it ASAP.**

**Q: Why in some places the documentation is missing or is not clear?**\
**A:** I am not pretending that I provided completely full documentation. If you have any questions and/or suggestions don't hesitate to contact me and I will answer you and will extend the documentation and Q&A section. **The easiest way is via message on [LinkedIn](https://bg.linkedin.com/in/dakov)**

**Q: Why the Database's schema is not completely normalized and there are data repeatings?**\
**A:** The reason for that is pretty simple because has no UI for reporting, and with pretty simple SELECT queries I can extract the reports of required data! :) 

# To-Do List

* Continuously improve the algorithm and bug fixing
* Extend the Email notifications and make them more human-readable
* Add Binance as a second crypto trader platform

# Release notes

#### v1.4.2 Version

**What's New**

* Improved version of the Trade algorithm
* Added Prices Queue included into prices calculation for the new orders 
* Improved configurable variables and gives more flexibility
* Added ability to for unlimited counts of order's level for certain pair
* Extended the logging with Serilog
* Improved code quality and documentation

**How to update from the previous version**

* Stop the App
* Replace the files with new ones
* Fill appSettings.json with your configurable values
* Start the App and enjoy! :)

#### v1.3.5 Version

**Minimum Viable Product**

* Good enough version of the trading algorithm
* Ability to self-configuration the pairs via the config file

# Author

#### Petar Dakov

* [LinkedIn](https://bg.linkedin.com/in/dakov)
* [Personal Website](http://dakov.net/)

# License

[![License](http://img.shields.io/:license-MIT-blue.svg)](https://licenses.nuget.org/MIT)