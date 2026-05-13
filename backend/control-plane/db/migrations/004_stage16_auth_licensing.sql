-- MiLuStudio Stage 16 account, session, device binding, and license schema.
-- These tables belong to the Control API / Infrastructure boundary. Electron and the installer must not run or own this schema.

create table if not exists accounts (
    id text primary key,
    phone text null,
    email text null,
    display_name text not null,
    password_hash text not null,
    status text not null check (status in ('active', 'locked', 'deleted')),
    created_at timestamptz not null,
    last_login_at timestamptz null,
    constraint ck_accounts_identifier_present check (phone is not null or email is not null)
);

create unique index if not exists ix_accounts_email
    on accounts (lower(email))
    where email is not null;

create unique index if not exists ix_accounts_phone
    on accounts (phone)
    where phone is not null;

create table if not exists devices (
    id text primary key,
    account_id text not null references accounts(id) on delete cascade,
    machine_fingerprint_hash text not null,
    device_name text not null,
    first_seen_at timestamptz not null,
    last_seen_at timestamptz not null,
    trusted boolean not null default true
);

create unique index if not exists ix_devices_account_fingerprint
    on devices (account_id, machine_fingerprint_hash);

create index if not exists ix_devices_account_id
    on devices (account_id, last_seen_at desc);

create table if not exists auth_sessions (
    id text primary key,
    account_id text not null references accounts(id) on delete cascade,
    device_id text not null references devices(id) on delete cascade,
    access_token_hash text not null,
    refresh_token_hash text not null,
    created_at timestamptz not null,
    last_seen_at timestamptz not null,
    expires_at timestamptz not null,
    revoked_at timestamptz null
);

create unique index if not exists ix_auth_sessions_access_token_hash
    on auth_sessions (access_token_hash);

create unique index if not exists ix_auth_sessions_refresh_token_hash
    on auth_sessions (refresh_token_hash);

create index if not exists ix_auth_sessions_account_id
    on auth_sessions (account_id, expires_at desc);

create table if not exists licenses (
    id text primary key,
    account_id text not null references accounts(id) on delete cascade,
    license_type text not null check (license_type in ('trial', 'paid', 'offline_signed')),
    plan text not null,
    activation_code_hash text null,
    status text not null check (status in ('active', 'expired', 'revoked')),
    starts_at timestamptz not null,
    expires_at timestamptz not null,
    max_devices integer not null check (max_devices > 0),
    created_at timestamptz not null,
    updated_at timestamptz not null
);

create index if not exists ix_licenses_account_id
    on licenses (account_id, updated_at desc);

create index if not exists ix_licenses_active
    on licenses (account_id, status, starts_at, expires_at);
