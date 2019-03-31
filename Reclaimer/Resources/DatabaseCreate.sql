create table cache_file (
    cache_id integer primary key autoincrement not null, 
    file_name varchar(512) not null, 
    build_string varchar(32) not null, 
    cache_type int not null, 
    priority int not null
);
    
create table tag_index (
    cache_id integer primary key not null,
    magic int not null,
    tag_count int not null,
    foreign key (cache_id) references cache_file(cache_id) on delete cascade
);

create table string_index (
    cache_id integer primary key not null,
    string_count int not null,
    foreign key (cache_id) references cache_file(cache_id) on delete cascade
);

create table index_item (
    cache_id integer not null,
    tag_id integer not null,
    meta_pointer int not null,
    path_id integer not null,
    class_code varchar(4) null,
    primary key (cache_id, tag_id),
    foreign key (cache_id) references tag_index(cache_id) on delete cascade,
    foreign key (path_id) references path(path_id) on delete cascade
) without rowid;

create table path (
    path_id integer primary key not null,
    value varchar(512) null
);

create table string_item (
    cache_id integer not null,
    string_id integer not null,
    value varchar(512) null,
    primary key (cache_id, string_id),
    foreign key (cache_id) references string_index(cache_id) on delete cascade
)