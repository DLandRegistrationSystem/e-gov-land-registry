// wwwroot/js/land.js
import { SuiClient, getFullnodeUrl } from "@mysten/sui/client";
import { Transaction } from "@mysten/sui/transactions";

// RPC client for Sui testnet
const client = new SuiClient({ url: getFullnodeUrl('testnet') });

// Wallet connection (Wallet Standard)
let walletProvider = null;

export async function connectWallet() {
    const provider = window?.wallets || window?.suiet || window?.suiWallet;
    if (!provider) {
        alert("Please install a Sui wallet (e.g., Sui Wallet or Suiet)!");
        throw new Error("No Sui wallet provider found");
    }
    walletProvider = provider;
    return true;
}

// Mint a new Land object by calling register_land
export async function registerLand({ packageId, module, adminCapId, landId, ownerHex, location, area }) {
    if (!walletProvider) await connectWallet();

    const tx = new Transaction();
    const cap = tx.object(adminCapId);

    const landObj = tx.moveCall({
        target: `${packageId}::${module}::register_land`,
        arguments: [
            cap,
            tx.pure.string(landId),
            tx.pure.address(ownerHex),
            tx.pure.string(location),
            tx.pure.u64(area)
        ]
    });

    // Ensure ownership is transferred to owner
    tx.transferObjects([landObj], ownerHex);

    const result = await walletProvider.signAndExecuteTransaction({
        transaction: tx,
        chain: 'sui:testnet'
    });
    return result;
}

// Transfer land ownership by owner
export async function transferLandByOwner({ packageId, module, landObjectId, newOwnerHex }) {
    if (!walletProvider) await connectWallet();

    const tx = new Transaction();
    const land = tx.object(landObjectId);

    tx.moveCall({
        target: `${packageId}::${module}::transfer_land_by_owner`,
        arguments: [land, tx.pure.address(newOwnerHex)]
    });

    const result = await walletProvider.signAndExecuteTransaction({
        transaction: tx,
        chain: 'sui:testnet'
    });
    return result;
}

export async function verifyOwnership({ objectId }) {
    try {
        return await client.getObject({ id: objectId, options: { showOwner: true, showContent: true } });
    } catch (err) {
        return { error: String(err) };
    }
}
