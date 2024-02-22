# AutoTrader Application

[![License](http://img.shields.io/:license-MIT-blue.svg)](https://licenses.nuget.org/MIT)

The **AutoTrader application** is a sophisticated automation service designed to execute cryptocurrency orders using the "Exchange Limit" type.
It continuously monitors the status of active and completed orders, promptly placing reciprocal orders with matching amounts at predefined intervals.

**Example:** Suppose we have a "Sale" order for 0.01 BTC that gets executed. In response, the AutoTrader will promptly initiate a purchase order
for the same BTC quantity, calculated at a price determined by the **ProfitRatio** coefficient. Subsequently, when the "Buy" order is fulfilled,
the AutoTrader will proceed to place another sell order for 0.01 BTC, again at a price determined by the **ProfitRatio** coefficient.

**Note:** The generated profit will not be automatically reinvested. You have the option to monitor and reinvest it manually, or withdraw it as desired.

As a developer and crypto enthusiast, I've developed and continue to enhance this application while keeping it free and open-source.
I firmly believe that by sharing our knowledge with others, we contribute to making the world a better place.

The primary goal behind creating this app is to realize my vision of generating passive income with minimal time spent monitoring the application.

`Introducing this app DOES NOT constitute a recommendation to engage in the Crypto World or pursue new investments.
The application is provided "AS IS" and I DO NOT accept responsibility for any losses resulting from the app or the fluctuations of the crypto bear market.`

`You are NOT required to send any funds to me or any other crypto account.
All of your funds will remain in YOUR personal account, fully under YOUR control.`

**But for your benefit, I'll gladly shoulder all the blame! :)**\
**And if you wish, feel free to grab me a beer.**

#### An expression of gratitude
For every donor, I offer the opportunity to reserve space for their name, project, or any other information they wish to share with my audience.
The donor's data will be prominently featured within Section **Friends and Supporters**.

This platform can also serve as an exchanger for advertisements. :)

#### Opportunities for donations
All wallets are facilitated and backed by **[Bitfinex](https://www.bitfinex.com/sign-up?refcode=kwqvP9OMT)**. Please review the current information.

* **Bitcoin (BTC)**\
Address - **3FqXqTzGhzxosERosz832F7rGDXMteMjRh**

* **Ethereum (ETH)**\
Address - **0x818E07acD7d75d1812B2E7067e417C5Bc80d327E**

* **XRP (XRP)** Depositing Ripple to Bitfinex requires BOTH a deposit address and a deposit Tag.\
Address - **rLW9gnQo7BQhU6igk5keqYnH3TVrCxGRzm**\
Tag - **2345006832**

* **Dogecoin (DOGE)**\
Address - **DNXrvtmrSWVyBKfmPizXQsreqLSRJQpqCv**

* **Bitcoin Cash Node (BCHN)** Depositing anything other than Bitcoin Cash WILL RESULT IN LOSS OF FUNDS. Bitfinex currently is not supporting automated chain splits.\
Address - **bitcoincash:qz4tu69cn50gryhdahemytrszdtnn9vwtqexclhleq**

* **Litecoin (LTC)**\
Address - **MD7ngCjhUbLNZfnvLdXkVu1LUkXwZMTHmF**

* **Monero (XMR)**\
Address - **86GNkvtYzbEZwq5MxpVvzP7kZ2tUgea5SKy8FTJ4S48fY8hz3DYBPWwJsLNz2woi7dQi34TYkcPDpZKh7iZLaaMLGHrSTwZ**

**Note:** It is advisable to review transaction fees before proceeding with a transaction and select the appropriate currency accordingly.
In some cases, fees can be exceptionally high.

# My experience with the AutoTrader

At the end of **December 2020, I launched the App** with an initial investment in USD spread across 36 pairs, managing 61 active orders.
Nearly a year later, by November 2021, I periodically reinvested profits and injected fresh funds, resulting in 37 active pairs and 99 orders.

During the first 7 months of operation, the AutoTrader executed nearly 1800 orders, yielding a 32% profit from the initial investment.
Over 11 months of uptime, the total executed orders surpassed 2370, accumulating a 48.7% profit from all deposits made during that time.

**Achieving a 48.7% profit in less than a year, with minimal support and price monitoring, is undoubtedly commendable and speaks to the effectiveness of the AutoTrader.**

The month with the highest volume of executed orders was January, closely followed by August, September, October, and November.
However, during periods of rapid market growth, the AutoTrader struggled to adapt to swiftly rising prices, resulting in some orders being left below the current market price.

`This is closely tied to the volatility of the cryptocurrency market and the settings in your Pairs' configuration.`

In the latest version, the trading algorithm has been refined to be more adaptive to market conditions.

Based on my observations of the crypto market in recent months, I've concluded that it's more advantageous to concentrate on a smaller number of pairs
with a higher count of active orders. This approach leads to more executed orders and, consequently, more frequent profits.

**Update, February 2024:** Currently, my pairs' configuration consists of 7 active pairs with a total of 198 orders.

# To use the AutoTrader effectively, follow these steps

1. Begin by downloading the AutoTrader source code and thoroughly reviewing it. **Remember, don't take my word for it at 100%! :)**
2. Proceed to execute the steps outlined in the **Prerequisites** section diligently.
3. Follow through with the steps provided in **To configure the settings in the appSettings.json file, follow these steps** meticulously.
4. Generate the Publish files from the project, either through Visual Studio or using commands.
5. Install the AutoTrader as you would any normal Windows Service.
6. Exercise patience and take pleasure in the process.
7. And last but not least, if you find value in and appreciate my work, **don't forget to grab me a beer! :)**

**Hint:** You can skip steps 4 and 5 and directly launch the AutoTrader, either via Visual Studio or using commands.

**Note:** Basic IT knowledge is assumed for this process.

# Prerequisites 

* You need to install MS SQL Server, with the latest free SQL Express version being suitable for your needs [Download it](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
* Optional: SMTP Server ( [I'm using the free SMTP server provided by Google.]( https://support.google.com/a/answer/176600) )
* You must have an account in **[Bitfinex](https://www.bitfinex.com/sign-up?refcode=kwqvP9OMT)** ( If you're new, I'd appreciate it if you could use my referral link! Referral code: kwqvP9OMT )
* You need to configure your **Key** and **Secret** in Bitfinex ( [How to set up them](https://support.bitfinex.com/hc/en-us/articles/115002349625-API-Key-Setup-Login) )
* **Hint:** I am using a Windows Server instance under AWS EC2 as part of [AWS Free Tier](https://aws.amazon.com/free/).
`It is entirely free for 12 months, but exercise caution when creating the VM as any missteps can incur costs.`
* **Hint:** Alternatively of MS SQL Server, you can use [Amazon RDS Free Tier](https://aws.amazon.com/rds/free/)
* **STRONG SUGGEST:** A recommended approach is to create and use a secondary **[Bitfinex](https://www.bitfinex.com/sign-up?refcode=kwqvP9OMT)** account,
containing only the funds you want the AutoTrader to operate with. I also employ this approach.

# To configure the settings in the appSettings.json file, follow these steps

#### MS SQL Server 
```
"SqlSettings": {
    "ConnectionString": "Data Source=<Set your server address>; Initial Catalog=AutoTrader; User Id=<Set your username>; Password=<Set your password>; TrustServerCertificate=True;"
}
```

#### SMTP Server settings for email notifications
```
"SmtpServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "<Set your username>",
    "Password": "<Set your password>",
    "Recipients": "<Set your recipients>",
    "IsActive": true
}
```

#### Cryptocurrency exchanges settings
```
"ExchangesSettings": [
    {
        "Name": "Bitfinex",
        "Client": {
            "Key": "<Set your key>",
            "Secret": "<Set your secret>"
        },
        "IsActive": true,
        "Pairs": [
            {
                // Examples will be provided below for clarification.
            }
        ]
    },
    {
        "Name": "Binance",
        "Client": {
            "Key": "<Set your key>",
            "Secret": "<Set your secret>"
        },
        "IsActive": false,
        "Pairs": [
            {
                // Examples will be provided below for clarification.
            }
        ]
    }
]
```

#### Profit ratio
For almost four years now (since 2020y), I've been actively trading via the AutoTrader and analyzing market movements. During this time,
I've settled on using this value for ProfitRatio as it aligns with my expectations and trading style. However, feel free to adjust it according to your preferences and goals.

```
"ProfitRatio": 1.01999
```

#### Additional settings
You have the option to maintain these values as they are, or if you prefer, you can review the code and modify them based on your own judgment.

```
"TimeBetweenIterationInSeconds": 20,
"CacheExpirationMultiplier": 15,
"HealthCheckIntervalInHours": 0,
"HealthCheckHours": [ 6, 19 ],
"PricesQueueSize": 3,
"MinRatioToNearestOrder": 1.00379,
```

#### Pairs
As previously mentioned, both the AutoTrader application and this article **do not serve as recommendations** for entering the crypto market.\
Therefore, I will **not publicly share** my coin pairs. Instead, I've provided a template and structure to guide you on how to fill the configuration.

The pairs' names you can get from Bitfinex for free.

```
"Pairs": [
    {
        "Name": "BTCUSD",
        "OrderAmount": 0.001,
        "MaxOrderLevelCount": 5,
        "IsActive": true
    },
    {
        "Name": "ETHUSD",
        "OrderAmount": 0.01,
        "MaxOrderLevelCount": 10,
        "IsActive": "true"
    },
    {
        "Name": "LTCUSD",
        "OrderAmount": 0.5,
        "MaxOrderLevelCount": 7,
        "IsActive": "true"
    }
]
```

# Notifications

* **Healthcheck** - At intervals defined by 'HealthCheckIntervalInHours' or specific hours as specified in 'HealthCheckHours', an email is sent containing
the count of active pairs and active orders.
* **Trade event** - An email containing information about each executed order and newly placed order is sent for every transaction.
* **Other** - In the event of an error or any other type of event not covered by the above two scenarios, the AutoTrader sends an email containing relevant information.

# Friends and Supporters

Looking forward to welcoming our first friends! :)

# Q&A Section

**Q: Who will manage my funds?**\
**A:** Rest assured, all funds will remain in **YOUR personal account**, fully under **YOUR control**. There's **NO need** to transfer funds to me or any other crypto account.

**Q: Is there a possibility of losing funds with crypto investments?**\
**A:** The crypto market is known for its high risk and volatility. While there's potential for significant losses in a short period, patience can lead to long-term gains.
Remember, stay calm and don't panic! :)

**Q: Is it possible to lose funds due to the AutoTrader?**\
**A:** In nearly for 4 years (since 2020y) of live trading with my real money, there hasn't been a single instance of losing funds due to the AutoTrader.
However, it's important to remember that the application is provided "AS IS" and I assume NO responsibility for any losses resulting from the app
or the volatile nature of the crypto market. Additionally, the AutoTrader is open source, allowing you to review and understand the algorithms used.
**If you encounter a bug, please don't hesitate to contact me, and I'll make every effort to resolve it as soon as possible.**

**Q: Why is documentation missing or unclear in certain areas?**\
**A:** I acknowledge that the documentation may not be exhaustive. If you have any questions or suggestions, please reach out to me.
I'm here to provide answers and expand the documentation and Q&A section as needed.
**The most convenient method is through direct messaging on [LinkedIn](https://bg.linkedin.com/in/dakov)**

**Q: Why isn't the database schema fully normalized, leading to data repetitions?**\
**A:** The reason is straightforward: there is no need for a fully normalized schema because there is no UI for reporting. Using simple SELECT queries,
I can easily extract the required data for reports.

# To-Do List

* Continuously enhance the algorithm and resolve bugs
* Fully implement support for trading on the Binance cryptocurrency exchange
* Introduce the capability for dynamic order amounts to be determined by the order cost

# Release Notes

#### v2.7.4 Version
**What's New**

* The database has been extended to support multiple cryptocurrency exchanges
* The .NET code was refactored to support multiple cryptocurrency exchanges by implementing the IExchangeService interface
* Upgraded to .NET 8
* Introduced email template for PlaceOrder notification
* Implemented caching to optimize database calls
* Enhanced trading algorithm and resolved bugs
* Added unit tests

**To update from the previous version, follow these steps**

* Stop the App
* Update files with the new versions
* Customize appSettings.json with Your preferences
* Launch the Application and experience the enhancements!

#### v1.4.2 Version
**What's New**

* Enhanced Trade algorithm
* Introduced Prices Queue for improved order price calculation
* Enhanced reconfigurability with more flexible variables
* Added support for unlimited order levels for specific pairs
* Integrated Serilog for expanded logging capabilities
* Improved code quality and documentation

**To update from the previous version, follow these steps**

* Stop the App
* Update files with the new versions
* Customize appSettings.json with Your preferences
* Launch the Application and experience the enhancements!

#### v1.3.5 Version
**Minimum Viable Product**

* Functional trading algorithm
* Pairs' self-configuration through the config file

# Author

#### Petar Dakov
* [LinkedIn](https://bg.linkedin.com/in/dakov)
* [Personal Website](http://dakov.net/)

# License

[![License](http://img.shields.io/:license-MIT-blue.svg)](https://licenses.nuget.org/MIT)