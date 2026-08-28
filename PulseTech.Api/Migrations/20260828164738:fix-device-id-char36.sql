-- Migration: fix-device-id-char36
-- Created: 2026-08-28T16:47:38+0200
--
-- Aligns the device_id type in the smart_pot schema with core.devices.id (char(36)).
-- Previously device_metadata.device_id and telemetry.device_id were varchar(50).

begin;

-- smart_pot.device_metadata --------------------------------------------------
alter table smart_pot.device_metadata
    drop constraint device_metadata_device_id_fkey;

alter table smart_pot.device_metadata
    alter column device_id type char(36) using device_id::char(36);

alter table smart_pot.device_metadata
    add constraint device_metadata_device_id_fkey
        foreign key (device_id) references core.devices
            on delete cascade;

-- smart_pot.telemetry ------------------------------------------------------------
alter table smart_pot.telemetry
    drop constraint telemetry_device_id_fkey;

alter table smart_pot.telemetry
    alter column device_id type char(36) using device_id::char(36);

alter table smart_pot.telemetry
    add constraint telemetry_device_id_fkey
        foreign key (device_id) references core.devices
            on delete cascade;

commit;
