##### \# IS.IotMonitoring — система моніторингу IoT-пристроїв і баз даних

##### 

##### Про що проєкт

##### 

##### Система імітує роботу IoT-датчиків температури, зберігає їхні показники в базах даних

##### і дозволяє спостерігати за роботою всієї системи та СУБД через Grafana.

##### 

##### \*\*DataGenerator\*\* імітує кілька датчиків температури: кожні 2 секунди надсилає показники,

##### &#x09;а приблизно раз на хвилину один випадковий датчик показує високу температуру,

##### &#x09;щоб було видно аномалії на графіках і у логах.

##### \*\*AuthService\*\*сервер авторизації: реєстрація, вхід, видача JWT-токенів (ролі `User` та `Admin`).

##### \*\*CoreApi\*\* основний сервіс: керування пристроями та прийом телеметрії.

##### &#x09;Доступ лише з дійсним токеном; створювати й видаляти пристрої може тільки `Admin`.

##### \*\*PostgreSQL\*\* зберігає користувачів (`iot\_auth`) і пристрої (`postgresql\_backend`),

##### &#x09;\*\*MongoDB\*\* показники датчиків (`iot\_monitoring.telemetry`).

##### \*\*Prometheus\*\* збирає метрики сервісів і самих СУБД,

##### \*\*Loki\*\* логи, 

##### \*\*Tempo\*\* трейси запитів, \*\*Grafana\*\* відображає дашборди.

##### 

##### \## Запуск

##### 

##### \*\*Потрібно:\*\* Docker Desktop.

##### 

##### ```bash

##### git clone https://github.com/Aqquuaa/IS.IotMonitoring

##### cd IS.IotMonitoring

##### docker compose up -d --build

##### ```

##### 

##### \### Адреси та облікові дані

##### 

##### | AuthService (Scalar) | http://localhost:5228/scalar | `admin` / `Admin` |

##### | CoreApi (Scalar) | http://localhost:5151/scalar | потрібен JWT-токен |

##### | Grafana | http://localhost:3000 | `admin` / `admin` |

##### | Prometheus | http://localhost:9090 | — |

##### | PostgreSQL | localhost:5433 | `postgres` / `postgres` |

##### | MongoDB | localhost:27018 | `admin` / `admin` |

##### 

##### \## Як перевірити

##### 

##### \### 1. Авторизація

##### 1\. Відкрийте http://localhost:5228/scalar `POST /api/auth/login` -> \*\*Test Request\*\* з тілом:

##### &#x20;  ```json

##### &#x20;  { "username": "admin", "password": "Admin" }

##### &#x20;  ```

##### &#x20;  Скопіюйте значення `accessToken`.

##### 2\. Відкрийте http://localhost:5151/scalar -> `GET /api/devices` → надішліть \*\*без токена\*\* -> `401 Unauthorized`.

##### 3\. На вкладці \*\*Headers\*\* додайте `Authorization: Bearer <токен>` і надішліть ще раз -> `200` і список датчиків.

##### 4\. Перевірка ролей: зареєструйте звичайного користувача (`POST /api/auth/register`, пароль від 8 символів),

##### увійдіть ним і спробуйте `POST /api/devices` -> `403 Forbidden` (створювати пристрої може лише Admin).

##### 

##### \### 2. Prometheus (збір і зберігання метрик)

##### \- http://localhost:9090/targets - усі цілі (`coreapi`, `authservice`, `postgres`, `mongodb`) мають стан \*\*UP\*\*.

##### \- Приклади запитів (вкладка \*\*Graph\*\*):

##### &#x20; - `iot\_device\_temperature\_celsius` - температура кожного датчика в часі;

##### &#x20; - `max\_over\_time(iot\_device\_temperature\_celsius\[10m])` - максимум за 10 хвилин;

##### &#x20; - `pg\_up`, `mongodb\_up` — доступність СУБД;

##### &#x20; - `rate(http\_requests\_received\_total\[1m])` - кількість запитів за секунду.

##### 

##### \### 3. Grafana (відображення)

##### Увійдіть у http://localhost:3000 -> \*\*Dashboards -> IoT\*\*:

##### \- \*\*Temperature monitoring\*\* — графік температури з порогом 35 °C;

##### \- \*\*Sensors overheating\*\* — кількість датчиків, що зараз перегріті;

##### \- \*\*High Temperature Warnings\*\* — логи попереджень про високу температуру.

##### 

##### \### 4. Loki (логи)

##### Grafana -> \*\*Explore\*\* -> джерело \*\*Loki\*\*:

##### \- `{service="coreapi"}` — усі логи CoreApi;

##### \- `{service="coreapi"} |= "High temperature"`- лише попередження про перегрів;

##### \- `{service="coreapi"} | json | StatusCode >= 400` - запити з помилками.

##### 

##### \### 5. Tempo (трейси)

##### Grafana -> \*\*Explore\*\* -> джерело \*\*Tempo\*\* -> \*\*Search\*\* -> Service Name `coreapi`.

##### Відкрийте будь-який запит `POST api/telemetry` - видно, скільки часу зайняла обробка запиту

##### і кожен SQL-запит до PostgreSQL.

