## Инструкция для контрибьютора

### Регистрация issue (обсуждений)

> TODO

### подача pull-request (запросов на изменение)

для проекта действуют такие же принципы как и для проекта **oscript.io** - подробнее по ссылке http://oscript.io/dev/getting-started. 

Перд отправкой `pull-request` пожалуйста изучите:

* [Требования к исходному коду на языке C#](https://github.com/EvilBeaver/OneScript/blob/develop/CODESTYLE.md)

для разработки используйте следующие IDE

* Visual Studio 2017 и выше
* Visual Studio Code c расширениями C#, 1C-Syntax, OScript.Debug
  * обратите внимание в этом случае вам придется использовать `dotnet` - экспериментальная разработка ведется под версией `dotnet-sdk-3.1`, стабильная под версией `dotnet-sdk-2.2`
* Rider 2017.8 и выше

для приемочного тестирования используйте

* Docker Engine 17-ce и выше (с docker-compose)

### Отладочный запуск в режиме Docker

Вашу разработческую версию можно собрать и запустить для приемочного тестирования в режиме Docker (https://docs.docker.com/engine/reference/run/) или Docker-Compose (https://docs.docker.com/compose/reference/up/)

#### Сборка и запуск локальной версии образа OneScript.Web

* откройте файл `OneScript.sln` в вашей IDE
* выполните команду `publish solution` в вашей IDE (VisualStudio or Rider)
* в корне репозитория выполните команду `docker-compose build` - в списке ваших образов появится образ docker `omvc-engine-developer` на базе официального образа `mono:5.10`

После чего для запуска Web приложений используйте команду `docker run -v <КаталогНахожденияФайла_main.os>:/app -p <НомерЛокальногоПорта>:5000` 
* в образе определена точка монтировани `/app` - в него монтируется локальный каталог исходников приложения на oscript
* в образе определён порт запуска `5000` процесса Web сервера `OneScript.WebHost.exe` - его необходимо связать с номером локального порта

Например :

##### Docker

* Linux 

```
docker run --rm -v `pwd`/examples/empty/src:/app -p 5000:5000 omvc-engine-developer
```

* Windows 

```
set CURPWD=%cd%
set CURPWD=%CURPWD:\=/%

docker run --rm -v %CURPWD%/examples/empty/src:/app -p 5000:5000 omvc-engine-developer
``` 

##### Docker-compose

используйте следующий пример кода в свойм docker-compose файле

```
version: '3'

services:
  empty-web-app-oscript:
    image: omvc-engine-developer
    ports:
    - 5000:5000
    volumes:
    - ./examples/empty/src:/app

```


