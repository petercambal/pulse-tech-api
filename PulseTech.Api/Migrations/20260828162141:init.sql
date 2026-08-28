-- Migration: testnovej-migracie
-- Created: 2026-08-28T16:21:41+0200

create table auth.users
(
    user_id       uuid                     default gen_random_uuid() not null
        primary key,
    email         varchar(255)                                       not null
        unique,
    password_hash varchar(255)                                       not null,
    is_active     boolean                  default true,
    created_at    timestamp with time zone default now()
);

alter table auth.users
    owner to admin;

create table core.applications
(
    id          uuid                     default gen_random_uuid() not null
        primary key,
    app_code    varchar(50)                                        not null
        unique,
    name        varchar(100)                                       not null,
    description text,
    created_at  timestamp with time zone default now()
);

alter table core.applications
    owner to admin;

create table core.devices
(
    id            char(36)                 default gen_random_uuid() not null
        primary key,
    owner_user_id uuid                                               not null
        references auth.users
            on delete cascade,
    name          varchar(100)                                       not null,
    is_active     boolean                  default true,
    created_at    timestamp with time zone default now(),
    app_id        uuid
        constraint fk_devices_application
            references core.applications
            on delete cascade
);

alter table core.devices
    owner to admin;

create index idx_devices_owner
    on core.devices (owner_user_id);

create table smart_pot.device_metadata
(
    device_id  varchar(50) not null
        primary key
        references core.devices
            on delete cascade,
    plant_name varchar(100)
);

alter table smart_pot.device_metadata
    owner to admin;

create table smart_pot.telemetry
(
    time          timestamp with time zone not null,
    device_id     varchar(50)              not null
        references core.devices
            on delete cascade,
    soil_moisture double precision,
    light_lux     double precision,
    air_temp_c    double precision
);

alter table smart_pot.telemetry
    owner to admin;

create index telemetry_time_idx
    on smart_pot.telemetry (time desc);
