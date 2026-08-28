-- Migration: add-user-role-and-auth
-- Created: 2026-08-28T17:26:17+0200
--
-- Adds RBAC support to the existing users table.
--
-- NOTE: the users table lives in schema "auth" (not "core") and its primary key
-- is "user_id" - it is referenced by core.devices.owner_user_id, so the table is
-- extended in place rather than renamed/recreated.

alter table auth.users
    add column if not exists role varchar(20) not null default 'User';

alter table auth.users
    drop constraint if exists users_role_check;

alter table auth.users
    add constraint users_role_check check (role in ('Admin', 'User'));

comment on column auth.users.role is 'RBAC role: Admin or User';
