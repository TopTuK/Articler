-- This table defines Orleans operational queries. Orleans uses these to manage its operations,
-- these are the only queries Orleans issues to the database.
-- These can be redefined (e.g. to provide non-destructive updates) provided the stated interface principles hold.
CREATE TABLE OrleansQuery
(
    QueryKey varchar(64) NOT NULL,
    QueryText varchar(8000) NOT NULL,

    CONSTRAINT OrleansQuery_Key PRIMARY KEY(QueryKey)
);
