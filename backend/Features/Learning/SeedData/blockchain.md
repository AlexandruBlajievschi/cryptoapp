## Learning goal
Describe how transactions become part of a shared history and why confirmation is not always finality.

## From request to shared record
A blockchain groups transactions into blocks. Each block references the previous block using a cryptographic hash. A hash is a computed fingerprint of data: changing the data changes the fingerprint with overwhelming probability. Hashing is not encryption and does not make public transaction data secret.

Computers running the network check transactions against its rules. A signature can demonstrate authorisation by a key; the network also checks matters such as whether the requested funds are available. Valid transactions still need to be ordered into the history the network accepts.

Consensus rules help participants agree on that history. In proof of work, miners compete using computation and other nodes verify the resulting blocks. In proof of stake, validators participate using stake under rules that can penalise specified misconduct. Neither design makes every participant honest or every application safe.

## Example
Alex attempts to spend the same fictional balance in two different transactions. Both messages might reach different computers first. Consensus rules determine which ordering is accepted, and balance rules prevent both spends from remaining valid in the accepted history. Simply emailing everyone a copy would not settle that disagreement.

## Common misconception
"One confirmation means a transfer can never change." Some chains can replace recent blocks. Finality describes the confidence or protocol conditions after which a recorded result is not expected to be reversed. Network conditions and the chain's rules matter; there is no universal confirmation count for every asset.

## Key takeaways
- Hash links make changes detectable; consensus decides the accepted history.
- A blockchain records what its rules accept, not whether an outside-world claim is true.
- Public records may be traceable, and transaction fees and finality vary by network.

## Related glossary terms
Blockchain; Hash; Consensus; Proof of work; Proof of stake; Finality; Transaction.
