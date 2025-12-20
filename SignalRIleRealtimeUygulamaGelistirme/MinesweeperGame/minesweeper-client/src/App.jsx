import React, { useState, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

const MinesweeperGame = () => {
  const [connection, setConnection] = useState(null);
  const [roomId, setRoomId] = useState('');
  const [playerName, setPlayerName] = useState('');
  const [isJoined, setIsJoined] = useState(false);
  const [gameStarted, setGameStarted] = useState(false);
  const [board, setBoard] = useState([]);
  const [players, setPlayers] = useState([]);
  const [messages, setMessages] = useState([]);
  const [chatInput, setChatInput] = useState('');
  const [gameFinished, setGameFinished] = useState(false);
  const messagesEndRef = useRef(null);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/gamehub')
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('SignalR bağlandı');

          connection.on('RoomJoined', (room) => {
            setIsJoined(true);
            setPlayers(room.players || []);
            if (room.status === 1) {
              setGameStarted(true);
              updateBoard(room.board);
            }
          });

          connection.on('PlayersUpdated', (updatedPlayers) => {
            setPlayers(updatedPlayers || []);
          });

          connection.on('PlayerJoined', (player) => {
            setMessages(prev => [...prev, { 
              playerName: 'Sistem', 
              message: `${player.name} odaya katıldı`, 
              timestamp: new Date() 
            }]);
          });

          connection.on('PlayerLeft', (playerName) => {
            setMessages(prev => [...prev, { 
              playerName: 'Sistem', 
              message: `${playerName} oyundan ayrıldı`, 
              timestamp: new Date() 
            }]);
          });

          connection.on('GameStarted', (boardData) => {
            setGameStarted(true);
            updateBoard(boardData);
            setMessages(prev => [...prev, { 
              playerName: 'Sistem', 
              message: 'Oyun başladı! İyi şanslar!', 
              timestamp: new Date() 
            }]);
          });

          connection.on('CellRevealed', (result) => {
            setBoard(prevBoard => {
              const newBoard = [...prevBoard];
              result.revealedCells.forEach(cell => {
                const idx = cell.row * 10 + cell.col;
                if (newBoard[idx]) {
                  newBoard[idx] = {
                    ...newBoard[idx],
                    isRevealed: true,
                    isMine: cell.isMine,
                    adjacentMines: cell.adjacentMines,
                    revealedBy: cell.revealedBy
                  };
                }
              });
              return newBoard;
            });

            if (result.hitMine) {
              setMessages(prev => [...prev, { 
                playerName: 'Sistem', 
                message: '💥 Mayına bastınız!', 
                timestamp: new Date() 
              }]);
            }
          });

          connection.on('FlagToggled', (cell) => {
            setBoard(prevBoard => {
              const newBoard = [...prevBoard];
              const idx = cell.row * 10 + cell.col;
              if (newBoard[idx]) {
                newBoard[idx] = { ...newBoard[idx], isFlagged: cell.isFlagged };
              }
              return newBoard;
            });
          });

          connection.on('MessageReceived', (msg) => {
            setMessages(prev => [...prev, msg]);
          });

          connection.on('PlayerEliminated', (playerName) => {
            setMessages(prev => [...prev, { 
              playerName: 'Sistem', 
              message: `💀 ${playerName} elendi!`, 
              timestamp: new Date() 
            }]);
          });

          connection.on('GameFinished', (finalPlayers) => {
            setGameFinished(true);
            setPlayers(finalPlayers);
            setMessages(prev => [...prev, { 
              playerName: 'Sistem', 
              message: '🏆 Oyun bitti!', 
              timestamp: new Date() 
            }]);
          });
        })
        .catch(err => console.error('SignalR bağlantı hatası:', err));
    }

    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, [connection]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const updateBoard = (boardData) => {
    const cells = [];
    for (let i = 0; i < boardData.rows; i++) {
      for (let j = 0; j < boardData.cols; j++) {
        const cellData = boardData.cells.find(c => c.row === i && c.col === j);
        cells.push(cellData || { row: i, col: j, isRevealed: false, isFlagged: false, isMine: false, adjacentMines: 0 });
      }
    }
    setBoard(cells);
  };

  const joinRoom = async () => {
    if (connection && roomId && playerName) {
      try {
        await connection.invoke('JoinRoom', roomId, playerName);
      } catch (err) {
        console.error('Odaya katılma hatası:', err);
      }
    }
  };

  const startGame = async () => {
    if (connection && roomId) {
      try {
        await connection.invoke('StartGame', roomId);
      } catch (err) {
        console.error('Oyun başlatma hatası:', err);
      }
    }
  };

  const handleCellClick = async (row, col) => {
    if (connection && gameStarted && !gameFinished) {
      try {
        await connection.invoke('RevealCell', roomId, row, col);
      } catch (err) {
        console.error('Hücre açma hatası:', err);
      }
    }
  };

  const handleCellRightClick = async (e, row, col) => {
    e.preventDefault();
    if (connection && gameStarted && !gameFinished) {
      try {
        await connection.invoke('ToggleFlag', roomId, row, col);
      } catch (err) {
        console.error('Bayrak koyma hatası:', err);
      }
    }
  };

  const sendMessage = async () => {
    if (connection && chatInput.trim()) {
      try {
        await connection.invoke('SendMessage', roomId, chatInput);
        setChatInput('');
      } catch (err) {
        console.error('Mesaj gönderme hatası:', err);
      }
    }
  };

  const getCellContent = (cell) => {
    if (!cell.isRevealed) {
      return cell.isFlagged ? '🚩' : '';
    }
    if (cell.isMine) {
      return '💣';
    }
    return cell.adjacentMines > 0 ? cell.adjacentMines : '';
  };

  const getCellStyle = (cell) => {
    const base = {
      width: '50px',
      height: '50px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontWeight: 'bold',
      fontSize: '18px',
      border: '3px solid #64748b',
      borderRadius: '6px',
      cursor: 'pointer',
      transition: 'all 0.2s ease',
      userSelect: 'none'
    };

    if (!cell.isRevealed) {
      return { 
        ...base, 
        background: 'linear-gradient(145deg, #a8b4c4, #8d99ab)',
        boxShadow: '3px 3px 6px rgba(0,0,0,0.3), -2px -2px 4px rgba(255,255,255,0.3)',
      };
    }
    if (cell.isMine) {
      return { 
        ...base, 
        background: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)', 
        color: 'white', 
        fontSize: '24px',
        boxShadow: '0 4px 15px rgba(239, 68, 68, 0.5)'
      };
    }

    const colors = {
      0: { bg: '#f3f4f6', color: '#000' },
      1: { bg: '#dbeafe', color: '#1e40af' },
      2: { bg: '#dcfce7', color: '#15803d' },
      3: { bg: '#fee2e2', color: '#b91c1c' },
      4: { bg: '#e0e7ff', color: '#4338ca' },
      5: { bg: '#fed7aa', color: '#c2410c' },
      6: { bg: '#cffafe', color: '#0e7490' },
      7: { bg: '#fae8ff', color: '#a21caf' },
      8: { bg: '#e5e7eb', color: '#1f2937' }
    };

    const colorSet = colors[cell.adjacentMines] || colors[0];
    return { ...base, backgroundColor: colorSet.bg, color: colorSet.color, fontWeight: '800' };
  };

  if (!isJoined) {
    return (
      <div style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '20px'
      }}>
        <div style={{
          backgroundColor: 'white',
          borderRadius: '20px',
          boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.4)',
          padding: '50px 40px',
          width: '100%',
          maxWidth: '450px'
        }}>
          <h1 style={{ 
            fontSize: '42px', 
            fontWeight: '900', 
            textAlign: 'center', 
            marginBottom: '40px', 
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            letterSpacing: '-1px'
          }}>
            💣 Mayın Tarlası
          </h1>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
            <input
              type="text"
              placeholder="Oda ID (örn: room1)"
              value={roomId}
              onChange={(e) => setRoomId(e.target.value)}
              style={{
                width: '100%',
                padding: '16px 20px',
                border: '2px solid #e5e7eb',
                borderRadius: '12px',
                fontSize: '16px',
                outline: 'none',
                transition: 'all 0.2s',
                boxSizing: 'border-box'
              }}
              onFocus={(e) => e.target.style.borderColor = '#667eea'}
              onBlur={(e) => e.target.style.borderColor = '#e5e7eb'}
            />
            <input
              type="text"
              placeholder="İsminiz"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && joinRoom()}
              style={{
                width: '100%',
                padding: '16px 20px',
                border: '2px solid #e5e7eb',
                borderRadius: '12px',
                fontSize: '16px',
                outline: 'none',
                transition: 'all 0.2s',
                boxSizing: 'border-box'
              }}
              onFocus={(e) => e.target.style.borderColor = '#667eea'}
              onBlur={(e) => e.target.style.borderColor = '#e5e7eb'}
            />
            <button
              onClick={joinRoom}
              style={{
                width: '100%',
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                color: 'white',
                fontWeight: 'bold',
                padding: '16px',
                borderRadius: '12px',
                border: 'none',
                cursor: 'pointer',
                fontSize: '18px',
                transition: 'transform 0.2s, box-shadow 0.2s',
                boxShadow: '0 4px 15px rgba(102, 126, 234, 0.4)'
              }}
              onMouseOver={(e) => {
                e.target.style.transform = 'translateY(-2px)';
                e.target.style.boxShadow = '0 6px 20px rgba(102, 126, 234, 0.5)';
              }}
              onMouseOut={(e) => {
                e.target.style.transform = 'translateY(0)';
                e.target.style.boxShadow = '0 4px 15px rgba(102, 126, 234, 0.4)';
              }}
            >
              🚀 Odaya Katıl
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '30px',
      boxSizing: 'border-box'
    }}>
      <div style={{ 
        width: '100%', 
        maxWidth: '1400px', 
        display: 'flex', 
        flexDirection: 'column',
        gap: '25px'
      }}>
        <div style={{
          background: 'rgba(255, 255, 255, 0.15)',
          backdropFilter: 'blur(10px)',
          borderRadius: '20px',
          padding: '20px',
          border: '1px solid rgba(255, 255, 255, 0.2)',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.2)'
        }}>
          <h1 style={{ 
            fontSize: '42px', 
            fontWeight: '900', 
            color: 'white', 
            textAlign: 'center', 
            margin: '0',
            textShadow: '2px 2px 8px rgba(0,0,0,0.3)',
            letterSpacing: '-1px'
          }}>
            💣 Mayın Tarlası
            <span style={{ 
              fontSize: '26px', 
              fontWeight: '600', 
              marginLeft: '20px', 
              opacity: 0.95,
              background: 'rgba(255, 255, 255, 0.2)',
              padding: '8px 20px',
              borderRadius: '12px',
              display: 'inline-block'
            }}>
              Oda: {roomId}
            </span>
          </h1>
        </div>
        
        <div style={{ display: 'flex', gap: '25px', alignItems: 'stretch' }}>
          {/* Sol Panel - Oyun Tahtası */}
          <div style={{ 
            flex: '0 0 580px',
            backgroundColor: 'white', 
            borderRadius: '24px', 
            boxShadow: '0 20px 60px rgba(0, 0, 0, 0.4)', 
            padding: '35px',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            border: '3px solid rgba(255, 255, 255, 0.3)'
          }}>
            {!gameStarted ? (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <div style={{ 
                  fontSize: '80px', 
                  marginBottom: '25px',
                  animation: 'pulse 2s ease-in-out infinite'
                }}>⏳</div>
                <p style={{ 
                  fontSize: '26px', 
                  color: '#6b7280', 
                  marginBottom: '35px', 
                  fontWeight: '600',
                  lineHeight: '1.4'
                }}>
                  Oyun başlamayı bekliyor...
                </p>
                <button
                  onClick={startGame}
                  style={{
                    background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                    color: 'white',
                    fontWeight: 'bold',
                    padding: '18px 60px',
                    borderRadius: '16px',
                    border: 'none',
                    cursor: 'pointer',
                    fontSize: '22px',
                    boxShadow: '0 8px 25px rgba(16, 185, 129, 0.5)',
                    transition: 'all 0.3s ease',
                    letterSpacing: '0.5px'
                  }}
                  onMouseOver={(e) => {
                    e.target.style.transform = 'translateY(-3px) scale(1.05)';
                    e.target.style.boxShadow = '0 12px 35px rgba(16, 185, 129, 0.6)';
                  }}
                  onMouseOut={(e) => {
                    e.target.style.transform = 'translateY(0) scale(1)';
                    e.target.style.boxShadow = '0 8px 25px rgba(16, 185, 129, 0.5)';
                  }}
                >
                  🎮 Oyunu Başlat
                </button>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '25px' }}>
                <div style={{ 
                  display: 'inline-grid', 
                  gridTemplateColumns: 'repeat(10, 50px)', 
                  gap: '3px', 
                  padding: '15px', 
                  background: 'linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%)', 
                  borderRadius: '16px',
                  boxShadow: 'inset 0 2px 10px rgba(0,0,0,0.1)'
                }}>
                  {board.map((cell, idx) => (
                    <div
                      key={idx}
                      onClick={() => handleCellClick(cell.row, cell.col)}
                      onContextMenu={(e) => handleCellRightClick(e, cell.row, cell.col)}
                      style={getCellStyle(cell)}
                      onMouseOver={(e) => {
                        if (!cell.isRevealed) {
                          e.target.style.backgroundColor = '#6b7280';
                        }
                      }}
                      onMouseOut={(e) => {
                        if (!cell.isRevealed) {
                          e.target.style.backgroundColor = '#9ca3af';
                        }
                      }}
                    >
                      {getCellContent(cell)}
                    </div>
                  ))}
                </div>
                
                {gameFinished && (
                  <div style={{ 
                    marginTop: '25px', 
                    background: 'linear-gradient(135deg, #fef3c7 0%, #fde68a 100%)', 
                    border: '4px solid #f59e0b', 
                    borderRadius: '16px', 
                    padding: '25px',
                    textAlign: 'center',
                    boxShadow: '0 8px 25px rgba(245, 158, 11, 0.4)'
                  }}>
                    <h3 style={{ fontSize: '32px', fontWeight: 'bold', marginBottom: '12px' }}>🏆 Oyun Bitti!</h3>
                    <p style={{ color: '#78350f', fontSize: '18px', fontWeight: '600' }}>Skor tablosuna bakın →</p>
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Sağ Panel */}
          <div style={{ flex: '1', display: 'flex', flexDirection: 'column', gap: '25px' }}>
            {/* Oyuncular */}
            <div style={{ 
              backgroundColor: 'white', 
              borderRadius: '24px', 
              boxShadow: '0 20px 60px rgba(0, 0, 0, 0.4)', 
              padding: '25px',
              border: '3px solid rgba(255, 255, 255, 0.3)'
            }}>
              <h2 style={{ 
                fontSize: '24px', 
                fontWeight: '900', 
                marginBottom: '20px', 
                color: '#1f2937',
                display: 'flex',
                alignItems: 'center',
                gap: '10px'
              }}>
                <span style={{
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent'
                }}>
                  👥 Oyuncular
                </span>
                <span style={{
                  fontSize: '18px',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  padding: '4px 12px',
                  borderRadius: '8px',
                  fontWeight: '700'
                }}>
                  {players.length}
                </span>
              </h2>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                {players.map((player, idx) => (
                  <div key={idx} style={{ 
                    padding: '18px 20px', 
                    borderRadius: '16px', 
                    background: player.isAlive 
                      ? 'linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%)' 
                      : 'linear-gradient(135deg, #fee2e2 0%, #fecaca 100%)',
                    border: '3px solid ' + (player.isAlive ? '#10b981' : '#ef4444'),
                    transition: 'all 0.3s ease',
                    boxShadow: player.isAlive 
                      ? '0 4px 15px rgba(16, 185, 129, 0.3)' 
                      : '0 4px 15px rgba(239, 68, 68, 0.3)'
                  }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: '800', fontSize: '18px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <span style={{ fontSize: '22px' }}>{player.isAlive ? '✅' : '💀'}</span>
                        {player.name}
                      </span>
                      <span style={{ 
                        fontSize: '20px', 
                        fontWeight: '900',
                        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                        WebkitBackgroundClip: 'text',
                        WebkitTextFillColor: 'transparent',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '5px'
                      }}>
                        {player.score} <span style={{ filter: 'grayscale(0)' }}>🎯</span>
                      </span>
                    </div>
                    {!player.isAlive && (
                      <span style={{ fontSize: '14px', color: '#dc2626', fontWeight: '700', marginTop: '5px', display: 'block' }}>
                        ⚠️ Elendi
                      </span>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Chat */}
            <div style={{ 
              backgroundColor: 'white', 
              borderRadius: '24px', 
              boxShadow: '0 20px 60px rgba(0, 0, 0, 0.4)', 
              padding: '25px',
              flex: 1,
              display: 'flex',
              flexDirection: 'column',
              minHeight: '400px',
              border: '3px solid rgba(255, 255, 255, 0.3)'
            }}>
              <h2 style={{ 
                fontSize: '24px', 
                fontWeight: '900', 
                marginBottom: '20px',
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent'
              }}>
                💬 Sohbet
              </h2>
              <div style={{ 
                background: 'linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%)', 
                borderRadius: '16px', 
                padding: '18px', 
                flex: 1,
                overflowY: 'auto', 
                marginBottom: '18px',
                minHeight: 0,
                boxShadow: 'inset 0 2px 10px rgba(0,0,0,0.1)'
              }}>
                {messages.map((msg, idx) => (
                  <div key={idx} style={{ 
                    marginBottom: '12px', 
                    lineHeight: '1.6',
                    padding: '10px 12px',
                    background: msg.playerName === 'Sistem' ? 'rgba(239, 68, 68, 0.1)' : 'white',
                    borderRadius: '10px',
                    borderLeft: '4px solid ' + (msg.playerName === 'Sistem' ? '#ef4444' : '#667eea')
                  }}>
                    <span style={{ 
                      fontWeight: '800', 
                      color: msg.playerName === 'Sistem' ? '#ef4444' : '#667eea',
                      fontSize: '15px'
                    }}>
                      {msg.playerName}
                    </span>
                    <span style={{ color: '#1f2937', marginLeft: '8px', fontSize: '15px' }}>{msg.message}</span>
                  </div>
                ))}
                <div ref={messagesEndRef} />
              </div>
              <div style={{ display: 'flex', gap: '12px' }}>
                <input
                  type="text"
                  value={chatInput}
                  onChange={(e) => setChatInput(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
                  placeholder="Mesajınız..."
                  style={{
                    flex: 1,
                    padding: '14px 18px',
                    border: '3px solid #e5e7eb',
                    borderRadius: '12px',
                    outline: 'none',
                    fontSize: '15px',
                    transition: 'all 0.3s ease'
                  }}
                  onFocus={(e) => {
                    e.target.style.borderColor = '#667eea';
                    e.target.style.boxShadow = '0 0 0 3px rgba(102, 126, 234, 0.1)';
                  }}
                  onBlur={(e) => {
                    e.target.style.borderColor = '#e5e7eb';
                    e.target.style.boxShadow = 'none';
                  }}
                />
                <button
                  onClick={sendMessage}
                  style={{
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    color: 'white',
                    fontWeight: 'bold',
                    padding: '14px 28px',
                    borderRadius: '12px',
                    border: 'none',
                    cursor: 'pointer',
                    fontSize: '18px',
                    transition: 'all 0.3s ease',
                    boxShadow: '0 4px 15px rgba(102, 126, 234, 0.4)'
                  }}
                  onMouseOver={(e) => {
                    e.target.style.transform = 'translateY(-2px) scale(1.05)';
                    e.target.style.boxShadow = '0 6px 20px rgba(102, 126, 234, 0.5)';
                  }}
                  onMouseOut={(e) => {
                    e.target.style.transform = 'translateY(0) scale(1)';
                    e.target.style.boxShadow = '0 4px 15px rgba(102, 126, 234, 0.4)';
                  }}
                >
                  📤
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MinesweeperGame;