// Build with your bundler, or drop directly if you’re not transpiling:
import { SuiClient, getFullnodeUrl } from '@mysten/sui/client';
import { Transaction } from '@mysten/sui/transactions';

const client = new SuiClient({ url: getFullnodeUrl('testnet') });

// DApp Kit exposes wallet discovery; you can also rely on window?.sui*
// For simplicity here, we expect a wallet that implements the Wallet Standard.
let activeWallet = null;

// Minimal connect using the wallet standard events:
export async function connectWallet() {
  // dApp Kit is preferred for production, but this shows the shape:
  const providers = window?.wallets || window?.suiet || window?.suiWallet;
  if (!providers) throw new Error('No Sui wallet found.');
  // In production, render a wallet picker (dApp Kit provides UI).
  activeWallet = providers;
  // Many wallets expose `standard:connect` or a direct connect method via dApp Kit.
  return true;
}

export async function registerLand({
  packageId, module, adminCapId,
  landId, ownerHex, location, area
}) {
  if (!activeWallet) await connectWallet();

  const tx = new Transaction();
  // Bring the AdminCap object into the tx
  const cap = tx.object(adminCapId);

  // Call: register_land(&AdminCap, land_id, owner, location, area, ctx) : Land
  const landObj = tx.moveCall({
    target: `${packageId}::${module}::register_land`,
    arguments: [
      cap,
      tx.pure.string(landId),
      tx.pure.address(ownerHex),
      tx.pure.string(location),
      tx.pure.u64(area),
    ],
  });

  // Transfer the minted Land object to the owner address
  tx.transferObjects([landObj], ownerHex);

  // Ask the wallet to sign+execute
  const result = await activeWallet.signAndExecuteTransaction({
    transaction: tx,
    chain: 'sui:testnet'
  });

  return result;
}

export async function transferLandByOwner({
  packageId, module, landObjectId, newOwnerHex
}) {
  if (!activeWallet) await connectWallet();

  const tx = new Transaction();
  const land = tx.object(landObjectId);

  tx.moveCall({
    target: `${packageId}::${module}::transfer_land_by_owner`,
    arguments: [land, tx.pure.address(newOwnerHex)],
  });

  const result = await activeWallet.signAndExecuteTransaction({
    transaction: tx,
    chain: 'sui:testnet'
  });
  return result;
}
