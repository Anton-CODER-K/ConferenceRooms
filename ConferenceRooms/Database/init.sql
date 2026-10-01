-- Ролі користувачів для обмеження по редагування залу і цін для нього
CREATE TABLE roles
(
    role_id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);


-- Користувачі
CREATE TABLE users
(
    user_id BIGSERIAL PRIMARY KEY,
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role_id BIGINT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_users_role
        FOREIGN KEY (role_id)
        REFERENCES roles(role_id)
);


-- Рефреш токен обов'язково для сесій JWT - токен робити дуже довгим небажано
CREATE TABLE refresh_tokens
(
    refresh_token_id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    token_hash TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMPTZ,

    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id)
        REFERENCES users(user_id)
        ON DELETE CASCADE
);


-- кімната яку можна орендувати
CREATE TABLE rooms
(
    room_id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    capacity INTEGER NOT NULL,
    hourly_rate NUMERIC(12, 2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,

    CONSTRAINT chk_rooms_capacity
        CHECK (capacity > 0),

    CONSTRAINT chk_rooms_hourly_rate
        CHECK (hourly_rate >= 0)
);


-- Доп послуги які можна взяти до оренди
CREATE TABLE services
(
    service_id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    price NUMERIC(12, 2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,

    CONSTRAINT chk_services_price
        CHECK (price >= 0)
);


-- Прив'язка послуг до опридільоного залу
CREATE TABLE room_services
(
    room_id BIGINT NOT NULL,
    service_id BIGINT NOT NULL,

    PRIMARY KEY (room_id, service_id),

    CONSTRAINT fk_room_services_room
        FOREIGN KEY (room_id)
        REFERENCES rooms(room_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_room_services_service
        FOREIGN KEY (service_id)
        REFERENCES services(service_id)
        ON DELETE CASCADE
);


-- статуси оренди створюю одельною таблицею шоб можна було удобніше працювати з нима
CREATE TABLE booking_statuses
(
    status_id SMALLSERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);


-- Основна таблиця
CREATE TABLE bookings
(
    booking_id BIGSERIAL PRIMARY KEY,

    room_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    status_id SMALLINT NOT NULL,

    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,

    base_price NUMERIC(12, 2) NOT NULL,
    discount NUMERIC(12, 2) NOT NULL DEFAULT 0,
    surcharge NUMERIC(12, 2) NOT NULL DEFAULT 0,
    total_price NUMERIC(12, 2) NOT NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,

    CONSTRAINT fk_bookings_room
        FOREIGN KEY (room_id)
        REFERENCES rooms(room_id),

    CONSTRAINT fk_bookings_user
        FOREIGN KEY (user_id)
        REFERENCES users(user_id),

    CONSTRAINT fk_bookings_status
        FOREIGN KEY (status_id)
        REFERENCES booking_statuses(status_id),

    -- Базові перевірки
    CONSTRAINT chk_bookings_time
        CHECK (start_time < end_time),

    CONSTRAINT chk_bookings_base_price
        CHECK (base_price >= 0),

    CONSTRAINT chk_bookings_discount
        CHECK (discount >= 0),

    CONSTRAINT chk_bookings_surcharge
        CHECK (surcharge >= 0),

    CONSTRAINT chk_bookings_total_price
        CHECK (total_price >= 0)
);


-- Послуги які були замовленні
CREATE TABLE booking_services
(
    booking_id BIGINT NOT NULL,
    service_id BIGINT NOT NULL,
    price NUMERIC(12, 2) NOT NULL,

    PRIMARY KEY (booking_id, service_id),

    CONSTRAINT fk_booking_services_booking
        FOREIGN KEY (booking_id)
        REFERENCES bookings(booking_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_booking_services_service
        FOREIGN KEY (service_id)
        REFERENCES services(service_id),

    CONSTRAINT chk_booking_services_price
        CHECK (price >= 0)
);


-- Історія змінити статусів не обовязково але в майбутньому буде полезно шоб бачити хто коли шо приняв або поміняв
CREATE TABLE booking_status_history
(
    booking_status_history_id BIGSERIAL PRIMARY KEY,

    booking_id BIGINT NOT NULL,
    status_id SMALLINT NOT NULL,
    changed_by_user BIGINT,
    changed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_booking_status_history_booking
        FOREIGN KEY (booking_id)
        REFERENCES bookings(booking_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_booking_status_history_status
        FOREIGN KEY (status_id)
        REFERENCES booking_statuses(status_id),

    CONSTRAINT fk_booking_status_history_user
        FOREIGN KEY (changed_by_user)
        REFERENCES users(user_id)
);


-- правило розцінування в яку годину
CREATE TABLE pricing_rules
(
    pricing_rule_id BIGSERIAL PRIMARY KEY,

    name TEXT NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,

    adjustment_percent NUMERIC(5, 2) NOT NULL DEFAULT 0,

    priority INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT chk_pricing_rule_time
        CHECK (start_time < end_time),

    CONSTRAINT chk_pricing_rule_priority
        CHECK (priority >= 0)
);


CREATE INDEX idx_users_role_id
    ON users(role_id);

CREATE INDEX idx_refresh_tokens_user_id
    ON refresh_tokens(user_id);

CREATE INDEX idx_bookings_room_id
    ON bookings(room_id);

CREATE INDEX idx_bookings_user_id
    ON bookings(user_id);

CREATE INDEX idx_bookings_status_id
    ON bookings(status_id);

CREATE INDEX idx_bookings_room_time
    ON bookings(room_id, start_time, end_time);

CREATE INDEX idx_booking_services_service_id
    ON booking_services(service_id);

CREATE INDEX idx_booking_status_history_booking_id
    ON booking_status_history(booking_id);

CREATE INDEX idx_pricing_rules_active
    ON pricing_rules(is_active);


INSERT INTO roles(role_id, name)
VALUES
    (1, 'User'),
    (2, 'Admin');


INSERT INTO booking_statuses(status_id, name)
VALUES
    (1, 'Pending'),
    (2, 'Confirmed'),
    (3, 'Cancelled'),
    (4, 'Completed');


INSERT INTO rooms(name, capacity, hourly_rate, is_active)
VALUES
    ('Зал A', 50, 2000.00, TRUE),
    ('Зал B', 100, 3500.00, TRUE),
    ('Зал C', 30, 1500.00, TRUE);


INSERT INTO services(name, price, is_active)
VALUES
    ('Проектор', 500.00, TRUE),
    ('Wi-Fi', 300.00, TRUE),
    ('Звукова система', 700.00, TRUE);

INSERT INTO room_services (room_id, service_id)
VALUES
    (1, 1), 
    (1, 2), 
    (1, 3), 
    (2, 1), 
    (2, 2), 
    (2, 3), 
    (3, 1), 
    (3, 2), 
    (3, 3); 