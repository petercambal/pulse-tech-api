-- Migration: add-telemetry-device-time-index
-- Created: 2026-08-28T16:57:28+0200
--
-- Speeds up "telemetry for a device over a time range" queries
-- (filter by device_id, ordered by time).

create index if not exists telemetry_device_time_idx
    on smart_pot.telemetry (device_id, time desc);
