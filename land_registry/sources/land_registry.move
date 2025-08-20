module land_registry::land_registry {
    use sui::object::{UID, Self};
    use sui::tx_context::{Self, TxContext};
    use sui::transfer;
    use sui::event;

    /// Admin capability (mint & forced transfer)
    public struct AdminCap has key, store { id: UID }

    /// Global shared registry (so anyone can call entry functions)
    public struct Registry has key { id: UID }

    /// A land NFT-like object
    public struct Land has key, store {
        id: UID,
        land_id: vector<u8>,
        owner: address,
        location: vector<u8>,
        area_ropani: u64,
    }

    /// History events (query off-chain by packageId & event type)
    public struct RegisteredEvent has copy, drop {
        land_id: vector<u8>,
        owner: address,
        location: vector<u8>,
        area_ropani: u64,
    }

    public struct TransferredEvent has copy, drop {
        land_id: vector<u8>,
        from: address,
        to: address,
    }

    // One-time init: creates Registry (shared) and AdminCap
    fun init(ctx: &mut TxContext) {
        let reg = Registry { id: object::new(ctx) };
        transfer::share_object(reg);

        let cap = AdminCap { id: object::new(ctx) };
        transfer::transfer(cap, tx_context::sender(ctx));
    }

    /// Mint land (admin only).
    public entry fun register_land(
        _cap: &AdminCap,
        land_id: vector<u8>,
        owner: address,
        location: vector<u8>,
        area_ropani: u64,
        ctx: &mut TxContext
    ) {
        let l = Land {
            id: object::new(ctx),
            land_id,
            owner,
            location,
            area_ropani,
        };
        event::emit(RegisteredEvent {
            land_id: l.land_id,
            owner: l.owner,
            location: l.location,
            area_ropani: l.area_ropani,
        });

        transfer::transfer(l, owner);
    }

    /// User-driven transfer: only current owner may transfer
    public entry fun transfer_land_by_owner(
        l: &mut Land,
        new_owner: address,
        ctx: &mut TxContext
    ) {
        let sender = tx_context::sender(ctx);
        assert!(l.owner == sender, 0); // Check if sender is the current owner
        let old = l.owner;
        l.owner = new_owner;
        event::emit(TransferredEvent {
            land_id: l.land_id,
            from: old,
            to: new_owner,
        });
    }

    /// Admin forced transfer (if ever needed)
    public entry fun transfer_land_by_admin(
        _cap: &AdminCap,
        l: &mut Land,
        new_owner: address,
    ) {
        let old = l.owner;
        l.owner = new_owner;
        event::emit(TransferredEvent {
            land_id: l.land_id,
            from: old,
            to: new_owner,
        });
    }

    /// Simple read helper (can be used by indexers / simulations)
    public fun owner_of(l: &Land): address { l.owner }
}