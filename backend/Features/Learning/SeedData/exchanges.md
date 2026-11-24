## Learning goal
Distinguish a market quote from an executable trade and understand exchange custody, fees and liquidity.

## Where market prices come from
An exchange is a venue or service through which participants trade assets. A centralised exchange commonly keeps its own customer account records and may hold assets in custody. A decentralised exchange typically uses smart contracts for swaps; this changes the trust model but does not remove code, asset or interface risks.

An exchange's internal balance update is not necessarily an on-chain transaction. A withdrawal to a blockchain can be a separate operation with its own network, address format, fee and confirmation rules. Sending an asset over an unsupported network can make recovery difficult or impossible.

Liquidity describes how readily an asset can be traded without moving its price substantially. The difference between quoted buying and selling prices is the spread. Slippage is the difference between an expected execution price and the actual one. Fees, spread and slippage can all affect the amount received.

## Example
A fictional coin's last trade was $10. Someone sees that quote and expects a simulated purchase of ten units to cost $100. If available sellers ask $10.10 and $10.20, the last-trade quote alone cannot guarantee the execution total. A separate fee could change the total again. CryptoGuide's simulated trade rules will be stated explicitly; its market quote is not a live order offer.

## Common misconception
"The price on a dashboard is a guaranteed price I can trade at." Quotes have a source and timestamp. They can be delayed, and different venues may show different prices. A service offering swaps can also be a fraudulent imitation of a known exchange.

## Key takeaways
- Separate an exchange account balance from an on-chain balance.
- Compare the network, custody model and total costs, not just a headline price.
- A market-data API supplies information; it does not execute a trade for CryptoGuide.

## Related glossary terms
Exchange; Custody; Liquidity; Transaction; Gas; Market capitalisation.
