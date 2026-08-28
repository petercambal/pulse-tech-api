-- Migration: mock-device-data
-- Created: 2026-08-28T16:41:41+0200
--
-- Vygeneruje mock time-series telemetriu pre smart_pot zariadenia vytvorene
-- v migracii init.sql (device_id 11111111-...-000000000001 az ...000000000005).
--
-- Rozsah: od zaciatku aktualneho roka (date_trunc('year', now())) do teraz,
-- jeden zaznam na zariadenie kazdu hodinu.
--
-- Migracia je idempotentna - najprv zmaze existujuce zaznamy pre tieto
-- zariadenia a nasledne ich znovu vygeneruje.

begin;

with mock_devices(id) as (
    values
        ('11111111-1111-4111-8111-000000000001'),
        ('11111111-1111-4111-8111-000000000002'),
        ('11111111-1111-4111-8111-000000000003'),
        ('11111111-1111-4111-8111-000000000004'),
        ('11111111-1111-4111-8111-000000000005')
)
delete from smart_pot.telemetry t
using mock_devices d
where t.device_id = d.id;

with mock_devices(id) as (
    values
        ('11111111-1111-4111-8111-000000000001'),
        ('11111111-1111-4111-8111-000000000002'),
        ('11111111-1111-4111-8111-000000000003'),
        ('11111111-1111-4111-8111-000000000004'),
        ('11111111-1111-4111-8111-000000000005')
),
hours as (
    select generate_series(
        date_trunc('year', now()),
        date_trunc('hour', now()),
        interval '1 hour'
    ) as time
)
insert into smart_pot.telemetry (time, device_id, soil_moisture, light_lux, air_temp_c)
select
    h.time,
    d.id,
    -- Vlhkost pody: pilovity priebeh - klesa ~5 dni z ~58 % na ~28 %,
    -- potom skok nahor (zalievanie), + sum.
    round((greatest(0, least(100,
        58
        - mod((extract(epoch from h.time) / 3600)::int, 120) * 0.25
        + (random() - 0.5) * 3
    )))::numeric, 2) as soil_moisture,
    -- Osvetlenie: cez den sinusovy priebeh s vrcholom na poludnie,
    -- v noci takmer nula, + sezonna zlozka a sum.
    round((greatest(0,
        case
            when extract(hour from h.time) between 6 and 20
                then 800 * sin((extract(hour from h.time) - 6) / 14.0 * pi())
                     * (0.6 + 0.4 * (extract(doy from h.time) / 365.0))
                     + random() * 100
            else random() * 5
        end
    ))::numeric, 2) as light_lux,
    -- Teplota vzduchu: zaklad 21 C + sezonna sinusoida (min v zime)
    -- + denna sinusoida (vrchol ~15:00) + sum.
    round((
        21
        + 4 * sin((extract(doy from h.time) / 365.0) * 2 * pi() - pi() / 2)
        + 3 * sin((extract(hour from h.time) - 15) / 24.0 * 2 * pi())
        + (random() - 0.5) * 1.5
    )::numeric, 2) as air_temp_c
from hours h
cross join mock_devices d;

commit;
