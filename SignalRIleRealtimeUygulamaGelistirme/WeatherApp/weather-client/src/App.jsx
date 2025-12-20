import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { Cloud, CloudRain, Sun, Wind, Droplets, Gauge, Clock, MapPin, Trash2 } from 'lucide-react';

const WeatherApp = () => {
  const [connection, setConnection] = useState(null);
  const [weatherData, setWeatherData] = useState({});
  const [subscribedCities, setSubscribedCities] = useState(['Istanbul']);
  const [isConnected, setIsConnected] = useState(false);
  const [newCity, setNewCity] = useState('');

  const popularCities = ['Istanbul', 'Ankara', 'Izmir', 'Antalya', 'Bursa', 'Adana', 'Gaziantep', 'Konya', 'Eskisehir'];

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/weatherhub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('✅ SignalR Connected!');
          setIsConnected(true);
          
          subscribedCities.forEach(city => {
            connection.invoke('SubscribeToCity', city);
          });

          connection.on('ReceiveWeather', (weather) => {
            console.log('🌤️ Weather received:', weather);
            setWeatherData(prev => ({
              ...prev,
              [weather.city]: weather
            }));
          });
        })
        .catch(err => {
          console.error('❌ SignalR Connection Error:', err);
          setIsConnected(false);
        });

      connection.onreconnecting(() => {
        console.log('🔄 Reconnecting...');
        setIsConnected(false);
      });

      connection.onreconnected(() => {
        console.log('✅ Reconnected!');
        setIsConnected(true);
      });

      return () => {
        connection.stop();
      };
    }
  }, [connection]);

  const subscribeToCity = (city) => {
    if (!subscribedCities.includes(city) && connection && isConnected) {
      connection.invoke('SubscribeToCity', city)
        .then(() => {
          setSubscribedCities([...subscribedCities, city]);
          console.log(`📍 Subscribed to ${city}`);
        })
        .catch(err => console.error('Error subscribing:', err));
    }
  };

  const unsubscribeFromCity = (city) => {
    if (connection && isConnected) {
      connection.invoke('UnsubscribeFromCity', city)
        .then(() => {
          setSubscribedCities(subscribedCities.filter(c => c !== city));
          const newData = { ...weatherData };
          delete newData[city];
          setWeatherData(newData);
          console.log(`🗑️ Unsubscribed from ${city}`);
        })
        .catch(err => console.error('Error unsubscribing:', err));
    }
  };

  const addCustomCity = () => {
    const cityName = newCity.trim();
    if (cityName && connection && isConnected) {
      subscribeToCity(cityName);
      setNewCity('');
    }
  };

  const getWeatherIconUrl = (iconPath) => {
    if (!iconPath) return null;
    return iconPath.startsWith('//') ? `https:${iconPath}` : iconPath;
  };

  const formatTime = (timestamp) => {
    return new Date(timestamp).toLocaleTimeString('tr-TR', { 
      hour: '2-digit', 
      minute: '2-digit',
      second: '2-digit'
    });
  };

  const getTemperatureColor = (temp) => {
    if (temp < 0) return 'text-blue-200';
    if (temp < 10) return 'text-blue-100';
    if (temp < 20) return 'text-green-100';
    if (temp < 30) return 'text-yellow-100';
    return 'text-red-200';
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-500 via-purple-500 to-pink-500 p-4 sm:p-6">
      <div className="max-w-7xl mx-auto">
        
        <div className="text-center mb-8 animate-fade-in">
          <div className="inline-block mb-4">
            <div className="text-7xl mb-2">🌤️</div>
          </div>
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-white mb-3 drop-shadow-lg">
            Canlı Hava Durumu
          </h1>
          <p className="text-lg text-white/90 mb-4">SignalR ile gerçek zamanlı hava takibi</p>
          
          <div className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full backdrop-blur-md border transition-all">
            <span className={`relative flex h-3 w-3 ${isConnected ? '' : 'opacity-50'}`}>
              <span className={`animate-ping absolute inline-flex h-full w-full rounded-full opacity-75 ${
                isConnected ? 'bg-green-400' : 'bg-red-400'
              }`}></span>
              <span className={`relative inline-flex rounded-full h-3 w-3 ${
                isConnected ? 'bg-green-500' : 'bg-red-500'
              }`}></span>
            </span>
            <span className={`font-semibold ${
              isConnected ? 'text-green-100' : 'text-red-100'
            }`}>
              {isConnected ? 'Bağlı' : 'Bağlantı Bekleniyor...'}
            </span>
          </div>
        </div>

        <div className="bg-white/10 backdrop-blur-xl rounded-3xl p-6 mb-8 border border-white/20 shadow-2xl">
          <div className="flex items-center gap-2 mb-4">
            <MapPin className="w-6 h-6 text-white" />
            <h2 className="text-2xl font-bold text-white">Şehir Ekle</h2>
          </div>
          
          <div className="flex flex-wrap gap-2 mb-4">
            {popularCities.map(city => (
              <button
                key={city}
                onClick={() => subscribeToCity(city)}
                disabled={subscribedCities.includes(city) || !isConnected}
                className={`px-4 py-2 rounded-xl font-medium transition-all transform hover:scale-105 disabled:cursor-not-allowed disabled:opacity-50 ${
                  subscribedCities.includes(city)
                    ? 'bg-green-500 text-white shadow-lg'
                    : 'bg-white/90 text-purple-600 hover:bg-white hover:shadow-lg'
                }`}
              >
                {city} {subscribedCities.includes(city) && '✓'}
              </button>
            ))}
          </div>

          <div className="flex gap-2">
            <input
              type="text"
              value={newCity}
              onChange={(e) => setNewCity(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && addCustomCity()}
              placeholder="Özel şehir ekle (örn: London, Tokyo)"
              disabled={!isConnected}
              className="flex-1 px-4 py-3 rounded-xl bg-white/20 text-white placeholder-white/60 border border-white/30 focus:outline-none focus:border-white focus:bg-white/30 transition-all disabled:opacity-50"
            />
            <button
              onClick={addCustomCity}
              disabled={!isConnected || !newCity.trim()}
              className="px-6 py-3 bg-white text-purple-600 rounded-xl font-bold hover:bg-purple-50 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl transform hover:scale-105"
            >
              Ekle
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {subscribedCities.map(city => {
            const weather = weatherData[city];
            return (
              <div 
                key={city} 
                className="bg-white/10 backdrop-blur-xl rounded-3xl p-6 border border-white/20 hover:bg-white/20 transition-all shadow-2xl hover:shadow-3xl transform hover:scale-105 hover:-translate-y-1"
              >
                <div className="flex justify-between items-start mb-4">
                  <h3 className="text-2xl font-bold text-white flex items-center gap-2">
                    <MapPin className="w-5 h-5" />
                    {city}
                  </h3>
                  <button
                    onClick={() => unsubscribeFromCity(city)}
                    className="text-white/60 hover:text-red-300 transition-colors p-1 hover:bg-red-500/20 rounded-lg"
                    title="Şehri kaldır"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>

                {weather ? (
                  <>
                    <div className="flex items-center justify-between mb-6">
                      <div className="flex-1">
                        <div className={`text-6xl font-bold ${getTemperatureColor(weather.temperature)} drop-shadow-lg`}>
                          {weather.temperature}°
                        </div>
                        <div className="text-white/80 text-sm mt-1">
                          Hissedilen: {weather.feelsLike}°
                        </div>
                      </div>
                      {weather.icon && (
                        <img 
                          src={getWeatherIconUrl(weather.icon)} 
                          alt={weather.description}
                          className="w-20 h-20 drop-shadow-lg"
                        />
                      )}
                    </div>

                    <div className="text-white/90 text-lg capitalize mb-6 font-medium">
                      {weather.description}
                    </div>

                    <div className="grid grid-cols-2 gap-4 mb-4">
                      <div className="flex items-center gap-2 bg-white/10 rounded-xl p-3">
                        <Droplets className="w-5 h-5 text-blue-200" />
                        <div>
                          <div className="text-xs text-white/60">Nem</div>
                          <div className="font-bold text-white">{weather.humidity}%</div>
                        </div>
                      </div>

                      <div className="flex items-center gap-2 bg-white/10 rounded-xl p-3">
                        <Wind className="w-5 h-5 text-green-200" />
                        <div>
                          <div className="text-xs text-white/60">Rüzgar</div>
                          <div className="font-bold text-white">{weather.windSpeed} km/h</div>
                        </div>
                      </div>

                      <div className="flex items-center gap-2 bg-white/10 rounded-xl p-3">
                        <Gauge className="w-5 h-5 text-purple-200" />
                        <div>
                          <div className="text-xs text-white/60">Basınç</div>
                          <div className="font-bold text-white">{weather.pressure} mb</div>
                        </div>
                      </div>

                      <div className="flex items-center gap-2 bg-white/10 rounded-xl p-3">
                        <Clock className="w-5 h-5 text-yellow-200" />
                        <div>
                          <div className="text-xs text-white/60">Güncelleme</div>
                          <div className="font-bold text-white text-xs">{formatTime(weather.timestamp)}</div>
                        </div>
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="flex flex-col items-center justify-center h-64 space-y-4">
                    <div className="animate-spin rounded-full h-12 w-12 border-4 border-white/30 border-t-white"></div>
                    <div className="text-white/80 text-center">
                      <div className="font-semibold">Veri yükleniyor...</div>
                      <div className="text-sm text-white/60 mt-1">{city} için hava durumu alınıyor</div>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {subscribedCities.length === 0 && (
          <div className="text-center py-20 bg-white/10 backdrop-blur-xl rounded-3xl border border-white/20">
            <Cloud className="w-32 h-32 text-white/40 mx-auto mb-6 animate-bounce" />
            <p className="text-white/80 text-2xl font-semibold mb-2">Henüz şehir eklemediniz</p>
            <p className="text-white/60 text-lg">Yukarıdan şehir ekleyerek hava durumunu görüntüleyin</p>
          </div>
        )}

        <div className="text-center mt-12 text-white/60 text-sm">
          <p>Powered by WeatherAPI.com & SignalR</p>
          <p className="mt-1">🌍 Gerçek zamanlı hava durumu takibi</p>
        </div>
      </div>
    </div>
  );
};

export default WeatherApp;